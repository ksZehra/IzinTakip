using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using IzinTakip.Domain;
using IzinTakip.Infrastructure.Email;
using IzinTakip.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace IzinTakip.Tests.Features.Kullanicilar;

public sealed partial class KullaniciYonetimiTests(DatabaseApiFactory factory) : IClassFixture<DatabaseApiFactory>
{
    [Fact]
    public async Task Olustur_Ik_CalisanaHesapAcarGeciciParolaIleIlkGiristeDegistirmeZorunlu()
    {
        var ik = await IkAsync();
        var admin = await SuperAdminAsync();
        var calisanId = await TestData.CalisanEkleAsync(admin, "Yeni", await TestData.DepartmanEkleAsync(admin, $"Departman-{Guid.NewGuid():N}"));
        var eposta = $"yeni-{calisanId}@izintakip.test";

        var yanit = await ik.PostAsJsonAsync(
            "/api/kullanicilar",
            new { eposta, calisanId, roller = new[] { SistemRolleri.Calisan } },
            TestContext.Current.CancellationToken);
        var kullanici = await yanit.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        var parola = GeciciParola(await factory.EmailSender.BekleAsync(eposta, "İzin Takip hesabınız açıldı"));
        var yeni = await TestAuth.OturumluClientAsync(factory, eposta, parola);
        var bildirimler = await yeni.GetAsync("/api/bildirimler", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, yanit.StatusCode);
        Assert.Equal(calisanId, kullanici.GetProperty("calisanId").GetInt32());
        Assert.True(kullanici.GetProperty("parolaDegistirmeli").GetBoolean());
        Assert.Equal(SistemRolleri.Calisan, Assert.Single(kullanici.GetProperty("roller").EnumerateArray()).GetString());
        Assert.Equal(HttpStatusCode.Forbidden, bildirimler.StatusCode);
    }

    [Fact]
    public async Task Olustur_IkKorumaliRolAtayamaz_403Doner()
    {
        var ik = await IkAsync();

        var yanit = await ik.PostAsJsonAsync(
            "/api/kullanicilar",
            new { eposta = $"ik2-{Guid.NewGuid():N}@izintakip.test", roller = new[] { SistemRolleri.IK } },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, yanit.StatusCode);
        Assert.Equal("Korumalı rolü yalnız SuperAdmin atayabilir.", await DetayAsync(yanit));
    }

    [Fact]
    public async Task Ik_KorumaliRoluOlanKullaniciya_DokunamazSuperAdminDokunur()
    {
        var ik = await IkAsync();
        var digerIkId = await TestAuth.KullaniciOlusturAsync(factory, $"ik-{Guid.NewGuid():N}@izintakip.test", SistemRolleri.IK);
        var admin = await SuperAdminAsync();

        var sifirlama = await ik.PostAsync($"/api/kullanicilar/{digerIkId}/parola-sifirla", null, TestContext.Current.CancellationToken);
        var pasif = await ik.PutAsJsonAsync($"/api/kullanicilar/{digerIkId}/aktiflik", new { aktif = false }, TestContext.Current.CancellationToken);
        var superAdminIle = await admin.PutAsJsonAsync($"/api/kullanicilar/{digerIkId}/aktiflik", new { aktif = false }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, sifirlama.StatusCode);
        Assert.Equal("Korumalı role sahip kullanıcıda değişiklik yapma yetkiniz yok.", await DetayAsync(sifirlama));
        Assert.Equal(HttpStatusCode.Forbidden, pasif.StatusCode);
        Assert.Equal(HttpStatusCode.OK, superAdminIle.StatusCode);
    }

    [Fact]
    public async Task RolleriDegistir_Ik_YeniYetkiOturumuKapatmadanGecerliOlur()
    {
        var ik = await IkAsync();
        var eposta = $"terfi-{Guid.NewGuid():N}@izintakip.test";
        var kullaniciId = await TestAuth.KullaniciOlusturAsync(factory, eposta, SistemRolleri.Calisan);
        var kullanici = await TestAuth.OturumluClientAsync(factory, eposta);

        var oncesi = await kullanici.GetAsync("/api/raporlar/bakiyeler", TestContext.Current.CancellationToken);
        var degisim = await ik.PutAsJsonAsync(
            $"/api/kullanicilar/{kullaniciId}/roller",
            new { roller = new[] { SistemRolleri.Yonetici } },
            TestContext.Current.CancellationToken);
        var sonrasi = await kullanici.GetAsync("/api/raporlar/bakiyeler", TestContext.Current.CancellationToken);
        var govde = await degisim.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, oncesi.StatusCode);
        Assert.Equal(SistemRolleri.Yonetici, Assert.Single(govde.GetProperty("roller").EnumerateArray()).GetString());
        Assert.Equal(HttpStatusCode.OK, sonrasi.StatusCode);
    }

    [Fact]
    public async Task ParolaSifirla_EskiParolaGecmezGeciciParolaGecer()
    {
        var ik = await IkAsync();
        var eposta = $"unutkan-{Guid.NewGuid():N}@izintakip.test";
        var kullaniciId = await TestAuth.KullaniciOlusturAsync(factory, eposta, SistemRolleri.Calisan);
        var client = factory.CreateClient();

        var yanit = await ik.PostAsync($"/api/kullanicilar/{kullaniciId}/parola-sifirla", null, TestContext.Current.CancellationToken);
        var parola = GeciciParola(await factory.EmailSender.BekleAsync(eposta, "İzin Takip parolanız sıfırlandı"));
        var eski = await client.PostAsJsonAsync("/api/auth/login", new { eposta, parola = TestAuth.VarsayilanParola }, TestContext.Current.CancellationToken);
        var yeni = await client.PostAsJsonAsync("/api/auth/login", new { eposta, parola }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, yanit.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, eski.StatusCode);
        Assert.True((await yeni.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken)).GetProperty("parolaDegistirmeli").GetBoolean());
    }

    [Fact]
    public async Task ParolaSifirla_GeciciParolaSuresiDolunca_GirisReddedilir()
    {
        var ik = await IkAsync();
        var eposta = $"gec-kalan-{Guid.NewGuid():N}@izintakip.test";
        var kullaniciId = await TestAuth.KullaniciOlusturAsync(factory, eposta, SistemRolleri.Calisan);

        await ik.PostAsync($"/api/kullanicilar/{kullaniciId}/parola-sifirla", null, TestContext.Current.CancellationToken);
        var mesaj = await factory.EmailSender.BekleAsync(eposta, "İzin Takip parolanız sıfırlandı");
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Kullanici>>();
            var kullanici = await userManager.FindByEmailAsync(eposta);
            Assert.InRange(kullanici!.GeciciParolaBitis!.Value, DateTime.UtcNow.AddDays(3).AddMinutes(-5), DateTime.UtcNow.AddDays(3));
            kullanici.GeciciParolaBitis = DateTime.UtcNow.AddMinutes(-1);
            await userManager.UpdateAsync(kullanici);
        }

        var yanit = await factory.CreateClient().PostAsJsonAsync(
            "/api/auth/login",
            new { eposta, parola = GeciciParola(mesaj) },
            TestContext.Current.CancellationToken);

        Assert.Contains("Parola 3 gün geçerlidir", WebUtility.HtmlDecode(mesaj.HtmlGovde));
        Assert.Equal(HttpStatusCode.Unauthorized, yanit.StatusCode);
        Assert.Equal("Geçici parolanızın süresi doldu. Parolanızı yeniden sıfırlaması için İK ile görüşün.", await DetayAsync(yanit));
    }

    [Fact]
    public async Task Aktiflik_PasiflestirilenKullanici_TokeniylaBile401Alir()
    {
        var ik = await IkAsync();
        var eposta = $"ayrilan-{Guid.NewGuid():N}@izintakip.test";
        var kullaniciId = await TestAuth.KullaniciOlusturAsync(factory, eposta, SistemRolleri.Calisan);
        var kullanici = await TestAuth.OturumluClientAsync(factory, eposta);

        var pasif = await ik.PutAsJsonAsync($"/api/kullanicilar/{kullaniciId}/aktiflik", new { aktif = false }, TestContext.Current.CancellationToken);
        var sonrasi = await kullanici.GetAsync("/api/auth/ben", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, pasif.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, sonrasi.StatusCode);
    }

    [Fact]
    public async Task Aktiflik_KendiHesabini_Pasiflestiremez()
    {
        var admin = await SuperAdminAsync();
        var ben = await admin.GetFromJsonAsync<JsonElement>("/api/auth/ben", TestContext.Current.CancellationToken);

        var yanit = await admin.PutAsJsonAsync(
            $"/api/kullanicilar/{ben.GetProperty("kullaniciId").GetInt32()}/aktiflik",
            new { aktif = false },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, yanit.StatusCode);
        Assert.Equal("Kendi hesabınızı pasifleştiremezsiniz.", await DetayAsync(yanit));
    }

    [Fact]
    public async Task Listele_Calisan_403Doner()
    {
        var eposta = $"merakli-{Guid.NewGuid():N}@izintakip.test";
        await TestAuth.KullaniciOlusturAsync(factory, eposta, SistemRolleri.Calisan);
        var client = await TestAuth.OturumluClientAsync(factory, eposta);

        var yanit = await client.GetAsync("/api/kullanicilar", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, yanit.StatusCode);
    }

    private async Task<HttpClient> IkAsync()
    {
        var eposta = $"ik-{Guid.NewGuid():N}@izintakip.test";
        await TestAuth.KullaniciOlusturAsync(factory, eposta, SistemRolleri.IK);
        return await TestAuth.OturumluClientAsync(factory, eposta);
    }

    private Task<HttpClient> SuperAdminAsync() =>
        TestAuth.OturumluClientAsync(factory, ApiFactory.SuperAdminEposta, ApiFactory.SuperAdminParola);

    private static string GeciciParola(EmailMessage mesaj) =>
        GeciciParolaKalibi().Match(mesaj.HtmlGovde).Groups[1].Value;

    private static async Task<string?> DetayAsync(HttpResponseMessage yanit) =>
        (await yanit.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken)).GetProperty("detail").GetString();

    [GeneratedRegex(@"parolanız: ([A-Za-z0-9]{12})\.")]
    private static partial Regex GeciciParolaKalibi();
}
