using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace IzinTakip.Tests.Features.IslemKayitlari;

public sealed class IslemKaydiListeTests(DatabaseApiFactory factory) : IClassFixture<DatabaseApiFactory>
{
    [Fact]
    public async Task Listele_CokluTabloVeIslemIleIlgiliAdi_YalnizEslesenleriGetirir()
    {
        var (kod, admin, calisanId) = await KayitlariKurAsync();

        var hepsi = await ListeAsync(admin, $"calisanId={calisanId}");
        var guncellemeler = await ListeAsync(admin, $"calisanId={calisanId}&islem=Guncellendi");
        var ikiTablo = await ListeAsync(admin, $"calisanId={calisanId}&tablo=Calisan&tablo=IzinHakki");
        var hakEklemesi = await ListeAsync(admin, $"calisanId={calisanId}&tablo=IzinHakki&islem=Eklendi");
        var adaGore = await ListeAsync(admin, $"ilgiliArama=Kayit{kod}&tablo=Calisan");

        Assert.Equal(3, hepsi.GetProperty("totalCount").GetInt32());
        Assert.Equal(["Calisan"], Tablolar(guncellemeler));
        Assert.Equal(3, ikiTablo.GetProperty("totalCount").GetInt32());
        Assert.Equal(["IzinHakki"], Tablolar(hakEklemesi));
        Assert.Equal(2, adaGore.GetProperty("totalCount").GetInt32());
    }

    [Fact]
    public async Task Listele_Sirala_SecilenAlanaGoreDonerVarsayilanYenidenEskiye()
    {
        var (_, admin, calisanId) = await KayitlariKurAsync();

        var varsayilan = await ListeAsync(admin, $"calisanId={calisanId}");
        var tariheGore = await ListeAsync(admin, $"calisanId={calisanId}&sort=tarih:asc");
        var tabloya = await ListeAsync(admin, $"calisanId={calisanId}&sort=tablo:desc");

        Assert.Equal(Idler(varsayilan).Reverse(), Idler(tariheGore));
        Assert.Equal(["IzinHakki", "Calisan", "Calisan"], Tablolar(tabloya));
    }

    [Theory]
    [InlineData("sort=kullaniciAdi:asc", "sort")]
    [InlineData("baslangic=2030-02-01&bitis=2030-01-01", "bitis")]
    public async Task Listele_GecersizSiralamaVeAralik_400Doner(string sorgu, string alan)
    {
        var admin = await SuperAdminAsync();

        var yanit = await admin.GetAsync($"/api/islem-kayitlari?{sorgu}", TestContext.Current.CancellationToken);
        var govde = await yanit.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, yanit.StatusCode);
        Assert.True(govde.GetProperty("errors").TryGetProperty(alan, out _));
    }

    // Çalışan eklenir (Calisan/Eklendi), güncellenir (Calisan/Guncellendi) ve hak satırı eklenir (IzinHakki/Eklendi).
    private async Task<(string Kod, HttpClient Admin, int CalisanId)> KayitlariKurAsync()
    {
        var kod = Guid.NewGuid().ToString("N")[..8];
        var admin = await SuperAdminAsync();
        var departmanId = await TestData.DepartmanEkleAsync(admin, $"Departman-{kod}");
        var calisanId = await TestData.CalisanEkleAsync(admin, $"Kayit{kod}", departmanId);
        var guncelle = await admin.PutAsJsonAsync(
            $"/api/calisanlar/{calisanId}",
            new { ad = $"Kayit{kod}", soyad = "Yeni", departmanId, iseGirisTarihi = "2020-01-06", dogumTarihi = "1990-05-15" },
            TestContext.Current.CancellationToken);
        guncelle.EnsureSuccessStatusCode();
        await TestData.HakEkleAsync(admin, calisanId, 480);
        return (kod, admin, calisanId);
    }

    private static async Task<JsonElement> ListeAsync(HttpClient istemci, string sorgu) =>
        await istemci.GetFromJsonAsync<JsonElement>($"/api/islem-kayitlari?{sorgu}", TestContext.Current.CancellationToken);

    private static string[] Tablolar(JsonElement liste) =>
        liste.GetProperty("items").EnumerateArray().Select(k => k.GetProperty("tablo").GetString()!).ToArray();

    private static int[] Idler(JsonElement liste) =>
        liste.GetProperty("items").EnumerateArray().Select(k => k.GetProperty("id").GetInt32()).ToArray();

    private Task<HttpClient> SuperAdminAsync() =>
        TestAuth.OturumluClientAsync(factory, ApiFactory.SuperAdminEposta, ApiFactory.SuperAdminParola);
}
