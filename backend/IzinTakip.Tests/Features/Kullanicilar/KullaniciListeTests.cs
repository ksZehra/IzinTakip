using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using IzinTakip.Domain;

namespace IzinTakip.Tests.Features.Kullanicilar;

public sealed class KullaniciListeTests(DatabaseApiFactory factory) : IClassFixture<DatabaseApiFactory>
{
    [Fact]
    public async Task Listele_CokluRolVeAktiflik_YalnizEslesenleriGetirir()
    {
        var (kod, admin) = await KullanicilariKurAsync();

        var ik = await ListeAsync(admin, $"arama={kod}&rol={SistemRolleri.IK}");
        var ikVeYonetici = await ListeAsync(admin, $"arama={kod}&rol={SistemRolleri.IK}&rol={SistemRolleri.Yonetici}");
        var pasifler = await ListeAsync(admin, $"arama={kod}&aktif=false");

        Assert.Equal([$"b-{kod}@izintakip.test"], Epostalar(ik));
        Assert.Equal([$"b-{kod}@izintakip.test", $"c-{kod}@izintakip.test"], Epostalar(ikVeYonetici));
        Assert.Equal([$"c-{kod}@izintakip.test"], Epostalar(pasifler));
    }

    [Fact]
    public async Task Listele_Sirala_SecilenAlanaGoreDoner()
    {
        var (kod, admin) = await KullanicilariKurAsync();

        var varsayilan = await ListeAsync(admin, $"arama={kod}");
        var epostaAzalan = await ListeAsync(admin, $"arama={kod}&sort=eposta:desc");
        var aktiflige = await ListeAsync(admin, $"arama={kod}&sort=aktif:asc");

        Assert.Equal([$"a-{kod}@izintakip.test", $"b-{kod}@izintakip.test", $"c-{kod}@izintakip.test"], Epostalar(varsayilan));
        Assert.Equal([$"c-{kod}@izintakip.test", $"b-{kod}@izintakip.test", $"a-{kod}@izintakip.test"], Epostalar(epostaAzalan));
        Assert.Equal($"c-{kod}@izintakip.test", Epostalar(aktiflige)[0]);
    }

    [Fact]
    public async Task Listele_CalisanAdinaGoreSiralar()
    {
        var kod = Guid.NewGuid().ToString("N")[..8];
        var admin = await SuperAdminAsync();
        var departmanId = await TestData.DepartmanEkleAsync(admin, $"Departman-{kod}");
        var zeynep = await TestData.CalisanEkleAsync(admin, "Zeynep", departmanId);
        var ahmet = await TestData.CalisanEkleAsync(admin, "Ahmet", departmanId);
        await TestAuth.KullaniciOlusturAsync(factory, $"a-{kod}@izintakip.test", SistemRolleri.Calisan, calisanId: zeynep);
        await TestAuth.KullaniciOlusturAsync(factory, $"b-{kod}@izintakip.test", SistemRolleri.Calisan, calisanId: ahmet);

        var liste = await ListeAsync(admin, $"arama={kod}&sort=calisanAdSoyad:asc");

        Assert.Equal([$"b-{kod}@izintakip.test", $"a-{kod}@izintakip.test"], Epostalar(liste));
    }

    [Fact]
    public async Task Listele_GecersizSiralama_400Doner()
    {
        var admin = await SuperAdminAsync();

        var yanit = await admin.GetAsync("/api/kullanicilar?sort=parola:asc", TestContext.Current.CancellationToken);
        var govde = await yanit.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, yanit.StatusCode);
        Assert.True(govde.GetProperty("errors").TryGetProperty("sort", out _));
    }

    // a: çalışan, b: İK, c: pasif yönetici.
    private async Task<(string Kod, HttpClient Admin)> KullanicilariKurAsync()
    {
        var kod = Guid.NewGuid().ToString("N")[..8];
        var admin = await SuperAdminAsync();
        await TestAuth.KullaniciOlusturAsync(factory, $"a-{kod}@izintakip.test", SistemRolleri.Calisan);
        await TestAuth.KullaniciOlusturAsync(factory, $"b-{kod}@izintakip.test", SistemRolleri.IK);
        var pasifId = await TestAuth.KullaniciOlusturAsync(factory, $"c-{kod}@izintakip.test", SistemRolleri.Yonetici);
        var pasif = await admin.PutAsJsonAsync($"/api/kullanicilar/{pasifId}/aktiflik", new { aktif = false }, TestContext.Current.CancellationToken);
        pasif.EnsureSuccessStatusCode();
        return (kod, admin);
    }

    private static async Task<JsonElement> ListeAsync(HttpClient istemci, string sorgu) =>
        await istemci.GetFromJsonAsync<JsonElement>($"/api/kullanicilar?{sorgu}", TestContext.Current.CancellationToken);

    private static string[] Epostalar(JsonElement liste) =>
        liste.GetProperty("items").EnumerateArray().Select(k => k.GetProperty("eposta").GetString()!).ToArray();

    private Task<HttpClient> SuperAdminAsync() =>
        TestAuth.OturumluClientAsync(factory, ApiFactory.SuperAdminEposta, ApiFactory.SuperAdminParola);
}
