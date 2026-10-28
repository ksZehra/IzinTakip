using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using IzinTakip.Domain;
using IzinTakip.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace IzinTakip.Tests.Features.Auth;

public sealed class AuthTests(VeritabanliApiFactory factory) : IClassFixture<VeritabanliApiFactory>
{
    [Fact]
    public async Task Giris_DogruBilgilerle_TokenVeSureBitisiDoner()
    {
        var yanit = await GirisYapAsync(ApiFactory.SuperAdminEposta, ApiFactory.SuperAdminParola);
        var govde = await yanit.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, yanit.StatusCode);
        Assert.False(string.IsNullOrEmpty(govde.GetProperty("accessToken").GetString()));
        Assert.True(govde.GetProperty("sureBitis").GetDateTime() > DateTime.UtcNow.AddHours(7));
        Assert.False(govde.GetProperty("parolaDegistirmeli").GetBoolean());
    }

    [Fact]
    public async Task Giris_HataliParolayla_401VeGenelMesajDoner()
    {
        var yanit = await GirisYapAsync(ApiFactory.SuperAdminEposta, "YanlisParola1");

        Assert.Equal(HttpStatusCode.Unauthorized, yanit.StatusCode);
        Assert.Equal("E-posta ya da parola hatalı. Üst üste hatalı denemede hesap birkaç dakika kilitlenir.", await DetayAsync(yanit));
    }

    [Fact]
    public async Task Giris_KayitsizEpostayla_HataliParolaylaAyniMesajiDoner()
    {
        var yanit = await GirisYapAsync("olmayan@izintakip.test", "YanlisParola1");

        Assert.Equal(HttpStatusCode.Unauthorized, yanit.StatusCode);
        Assert.Equal("E-posta ya da parola hatalı. Üst üste hatalı denemede hesap birkaç dakika kilitlenir.", await DetayAsync(yanit));
    }

    [Fact]
    public async Task Giris_BesHataliDenemedenSonra_DogruParolaylaBileAyniMesajlaReddedilir()
    {
        const string eposta = "kilit@izintakip.test";
        await TestKimlik.KullaniciOlusturAsync(factory, eposta, SistemRolleri.Calisan);
        for (var deneme = 0; deneme < 5; deneme++)
        {
            await GirisYapAsync(eposta, "YanlisParola1");
        }

        var yanit = await GirisYapAsync(eposta, TestKimlik.VarsayilanParola);

        Assert.Equal(HttpStatusCode.Unauthorized, yanit.StatusCode);
        Assert.Equal("E-posta ya da parola hatalı. Üst üste hatalı denemede hesap birkaç dakika kilitlenir.", await DetayAsync(yanit));
    }

    [Fact]
    public async Task Giris_BosEpostayla_400VeAlanHatasiDoner()
    {
        var yanit = await GirisYapAsync(string.Empty, "Parola2026");
        var govde = await yanit.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, yanit.StatusCode);
        Assert.Equal("E-posta zorunludur.", govde.GetProperty("errors").GetProperty("eposta")[0].GetString());
    }

    [Fact]
    public async Task Ben_Tokensiz_401VeTurkceMesajDoner()
    {
        var yanit = await factory.CreateClient().GetAsync("/api/auth/ben", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, yanit.StatusCode);
        Assert.Equal("application/problem+json", yanit.Content.Headers.ContentType?.MediaType);
        Assert.Equal("Oturum açmanız gerekiyor ya da oturumunuzun süresi doldu.", await DetayAsync(yanit));
    }

    [Fact]
    public async Task Ben_SuperAdminIcin_RolVeTumYetkileriDoner()
    {
        var client = await TestKimlik.OturumluClientAsync(factory, ApiFactory.SuperAdminEposta, ApiFactory.SuperAdminParola);

        var govde = await client.GetFromJsonAsync<JsonElement>("/api/auth/ben", TestContext.Current.CancellationToken);
        var yetkiler = govde.GetProperty("yetkiler").EnumerateArray().Select(y => y.GetString()).ToList();

        Assert.Equal(ApiFactory.SuperAdminEposta, govde.GetProperty("gorunenAd").GetString());
        Assert.Contains(SistemRolleri.SuperAdmin, govde.GetProperty("roller").EnumerateArray().Select(r => r.GetString()));
        Assert.Contains(Yetkiler.RolYonetimi, yetkiler);
        Assert.Equal(Yetkiler.Atanabilir.Count + 1, yetkiler.Count);
    }

    [Fact]
    public async Task YetkiliUc_YetkisiOlmayanKullaniciya_403VeTurkceMesajDoner()
    {
        await TestKimlik.KullaniciOlusturAsync(factory, "calisan@izintakip.test", SistemRolleri.Calisan);
        var client = await TestKimlik.OturumluClientAsync(factory, "calisan@izintakip.test");

        var yanit = await client.GetAsync("/test/yetki/rapor", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, yanit.StatusCode);
        Assert.Equal("Bu işlem için yetkiniz yok.", await DetayAsync(yanit));
    }

    [Fact]
    public async Task YetkiliUc_YetkisiOlanKullaniciya_200Doner()
    {
        await TestKimlik.KullaniciOlusturAsync(factory, "yonetici@izintakip.test", SistemRolleri.Yonetici);
        var client = await TestKimlik.OturumluClientAsync(factory, "yonetici@izintakip.test");

        var yanit = await client.GetAsync("/test/yetki/rapor", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, yanit.StatusCode);
    }

    [Fact]
    public async Task RolYonetimi_IkYeKapali_SuperAdmineAcik()
    {
        await TestKimlik.KullaniciOlusturAsync(factory, "ik@izintakip.test", SistemRolleri.IK);
        var ik = await TestKimlik.OturumluClientAsync(factory, "ik@izintakip.test");
        var superAdmin = await TestKimlik.OturumluClientAsync(factory, ApiFactory.SuperAdminEposta, ApiFactory.SuperAdminParola);

        var ikYaniti = await ik.GetAsync("/test/yetki/rol-yonetimi", TestContext.Current.CancellationToken);
        var superAdminYaniti = await superAdmin.GetAsync("/test/yetki/rol-yonetimi", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, ikYaniti.StatusCode);
        Assert.Equal(HttpStatusCode.OK, superAdminYaniti.StatusCode);
    }

    [Fact]
    public async Task ParolaDegistirmeliKullanici_ParolasiniDegistirenceYetkiliUcaGiremez()
    {
        await TestKimlik.KullaniciOlusturAsync(factory, "yeni@izintakip.test", SistemRolleri.Yonetici, parolaDegistirmeli: true);
        var client = await TestKimlik.OturumluClientAsync(factory, "yeni@izintakip.test");

        var oncesi = await client.GetAsync("/test/yetki/rapor", TestContext.Current.CancellationToken);
        var yetkisizUcOncesi = await client.GetAsync("/api/bildirimler", TestContext.Current.CancellationToken);
        var ben = await client.GetAsync("/api/auth/ben", TestContext.Current.CancellationToken);
        var degistir = await client.PostAsJsonAsync(
            "/api/auth/parola-degistir",
            new { mevcutParola = TestKimlik.VarsayilanParola, yeniParola = "YeniParola2026" },
            TestContext.Current.CancellationToken);
        var eskiTokenla = await client.GetAsync("/test/yetki/rapor", TestContext.Current.CancellationToken);
        var yeniOturum = await TestKimlik.OturumluClientAsync(factory, "yeni@izintakip.test", "YeniParola2026");
        var sonrasi = await yeniOturum.GetAsync("/test/yetki/rapor", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, oncesi.StatusCode);
        Assert.Equal("Devam etmek için önce parolanızı değiştirin.", await DetayAsync(oncesi));
        Assert.Equal(HttpStatusCode.Forbidden, yetkisizUcOncesi.StatusCode);
        Assert.Equal(HttpStatusCode.OK, ben.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, degistir.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, eskiTokenla.StatusCode);
        Assert.Equal(HttpStatusCode.OK, sonrasi.StatusCode);
    }

    [Fact]
    public async Task Cikis_VerilmisTokenlarGecersizOlur()
    {
        await TestKimlik.KullaniciOlusturAsync(factory, "cikan@izintakip.test", SistemRolleri.Calisan);
        var birinciCihaz = await TestKimlik.OturumluClientAsync(factory, "cikan@izintakip.test");
        var ikinciCihaz = await TestKimlik.OturumluClientAsync(factory, "cikan@izintakip.test");

        var cikis = await birinciCihaz.PostAsync("/api/auth/cikis", null, TestContext.Current.CancellationToken);
        var birinci = await birinciCihaz.GetAsync("/api/auth/ben", TestContext.Current.CancellationToken);
        var ikinci = await ikinciCihaz.GetAsync("/api/auth/ben", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, cikis.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, birinci.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, ikinci.StatusCode);
    }

    [Fact]
    public async Task ParolaDegistir_MevcutParolaHataliysa_422Doner()
    {
        await TestKimlik.KullaniciOlusturAsync(factory, "parola@izintakip.test", SistemRolleri.Calisan);
        var client = await TestKimlik.OturumluClientAsync(factory, "parola@izintakip.test");

        var yanit = await client.PostAsJsonAsync(
            "/api/auth/parola-degistir",
            new { mevcutParola = "YanlisParola1", yeniParola = "YeniParola2026" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, yanit.StatusCode);
        Assert.Equal("Mevcut parola hatalı.", await DetayAsync(yanit));
    }

    [Fact]
    public async Task PasifKullanici_GecerliTokenlaBile401Alir()
    {
        var kullaniciId = await TestKimlik.KullaniciOlusturAsync(factory, "pasif@izintakip.test", SistemRolleri.Calisan);
        var client = await TestKimlik.OturumluClientAsync(factory, "pasif@izintakip.test");

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Kullanici>>();
            var kullanici = await userManager.FindByIdAsync(kullaniciId.ToString(System.Globalization.CultureInfo.InvariantCulture));
            kullanici!.Aktif = false;
            await userManager.UpdateAsync(kullanici);
            scope.ServiceProvider.GetRequiredService<KullaniciYetkiOkuyucu>().Unut(kullaniciId);
        }

        var yanit = await client.GetAsync("/api/auth/ben", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, yanit.StatusCode);
    }

    [Fact]
    public async Task BilinmeyenAdres_OturumluIstekte404AyniBicimdeDoner()
    {
        var client = await TestKimlik.OturumluClientAsync(factory, ApiFactory.SuperAdminEposta, ApiFactory.SuperAdminParola);

        var yanit = await client.GetAsync("/boyle-bir-adres-yok", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, yanit.StatusCode);
        Assert.Equal("application/problem+json", yanit.Content.Headers.ContentType?.MediaType);
    }

    private Task<HttpResponseMessage> GirisYapAsync(string eposta, string parola) =>
        factory.CreateClient().PostAsJsonAsync("/api/auth/login", new { eposta, parola }, TestContext.Current.CancellationToken);

    private static async Task<string?> DetayAsync(HttpResponseMessage yanit) =>
        (await yanit.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken)).GetProperty("detail").GetString();
}
