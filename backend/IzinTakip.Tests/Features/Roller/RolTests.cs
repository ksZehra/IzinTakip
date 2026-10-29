using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using IzinTakip.Domain;

namespace IzinTakip.Tests.Features.Roller;

public sealed class RolTests(DatabaseApiFactory factory) : IClassFixture<DatabaseApiFactory>
{
    [Fact]
    public async Task Olustur_SuperAdmin_RolAcarYetkiAtanmisKullaniciHemenKullanir()
    {
        var admin = await SuperAdminAsync();
        var (kullaniciId, kullanici) = await KullaniciAsync();
        var ad = $"Raporcu-{Guid.NewGuid():N}"[..20];

        var olustur = await admin.PostAsJsonAsync(
            "/api/roller",
            new { ad, korumali = false, yetkiler = new[] { Yetkiler.Rapor.Gor } },
            TestContext.Current.CancellationToken);
        var rol = await olustur.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        await admin.PutAsJsonAsync($"/api/kullanicilar/{kullaniciId}/roller", new { roller = new[] { ad } }, TestContext.Current.CancellationToken);
        var rapor = await kullanici.GetAsync("/api/raporlar/bakiyeler", TestContext.Current.CancellationToken);
        var kaldir = await admin.PutAsJsonAsync(
            $"/api/roller/{rol.GetProperty("id").GetInt32()}",
            new { ad, korumali = false, yetkiler = Array.Empty<string>() },
            TestContext.Current.CancellationToken);
        var kaldirildiktanSonra = await kullanici.GetAsync("/api/raporlar/bakiyeler", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, olustur.StatusCode);
        Assert.Equal(Yetkiler.Rapor.Gor, Assert.Single(rol.GetProperty("yetkiler").EnumerateArray()).GetString());
        Assert.False(rol.GetProperty("sistemRolu").GetBoolean());
        Assert.Equal(HttpStatusCode.OK, rapor.StatusCode);
        Assert.Equal(HttpStatusCode.OK, kaldir.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, kaldirildiktanSonra.StatusCode);
    }

    [Fact]
    public async Task Olustur_Ik_403Doner()
    {
        var eposta = $"ik-{Guid.NewGuid():N}@izintakip.test";
        await TestAuth.KullaniciOlusturAsync(factory, eposta, SistemRolleri.IK);
        var ik = await TestAuth.OturumluClientAsync(factory, eposta);

        var olustur = await ik.PostAsJsonAsync("/api/roller", new { ad = "Yeni", korumali = false, yetkiler = Array.Empty<string>() }, TestContext.Current.CancellationToken);
        var liste = await ik.GetAsync("/api/roller", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, olustur.StatusCode);
        Assert.Equal(HttpStatusCode.OK, liste.StatusCode);
    }

    [Fact]
    public async Task Olustur_AtanamazYetki_400Doner()
    {
        var admin = await SuperAdminAsync();

        var yanit = await admin.PostAsJsonAsync(
            "/api/roller",
            new { ad = "Hileli", korumali = false, yetkiler = new[] { Yetkiler.RolYonetimi } },
            TestContext.Current.CancellationToken);
        var govde = await yanit.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, yanit.StatusCode);
        Assert.Equal("'Rol.Yonet' atanabilir bir yetki değil.", govde.GetProperty("errors").GetProperty("yetkiler[0]")[0].GetString());
    }

    [Fact]
    public async Task Olustur_AyniAd_422Doner()
    {
        var admin = await SuperAdminAsync();

        var yanit = await admin.PostAsJsonAsync(
            "/api/roller",
            new { ad = SistemRolleri.Yonetici, korumali = false, yetkiler = Array.Empty<string>() },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, yanit.StatusCode);
    }

    [Fact]
    public async Task SistemRolleri_SuperAdminDegismezAdiDegismezSilinmez()
    {
        var admin = await SuperAdminAsync();
        var roller = await admin.GetFromJsonAsync<JsonElement>("/api/roller", TestContext.Current.CancellationToken);
        int Id(string ad) => roller.EnumerateArray().Single(r => r.GetProperty("ad").GetString() == ad).GetProperty("id").GetInt32();

        var superAdmin = await admin.PutAsJsonAsync(
            $"/api/roller/{Id(SistemRolleri.SuperAdmin)}",
            new { ad = SistemRolleri.SuperAdmin, korumali = false, yetkiler = Array.Empty<string>() },
            TestContext.Current.CancellationToken);
        var adDegisimi = await admin.PutAsJsonAsync(
            $"/api/roller/{Id(SistemRolleri.Yonetici)}",
            new { ad = "Müdür", korumali = false, yetkiler = new[] { Yetkiler.IzinTalebi.Yonet } },
            TestContext.Current.CancellationToken);
        var silme = await admin.DeleteAsync($"/api/roller/{Id(SistemRolleri.Calisan)}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, superAdmin.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, adDegisimi.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, silme.StatusCode);
    }

    [Fact]
    public async Task Sil_KullanicisiOlanRolSilinmezBosRolSilinir()
    {
        var admin = await SuperAdminAsync();
        var (kullaniciId, _) = await KullaniciAsync();
        var dolu = await RolAcAsync(admin);
        var bos = await RolAcAsync(admin);
        await admin.PutAsJsonAsync($"/api/kullanicilar/{kullaniciId}/roller", new { roller = new[] { dolu.Ad } }, TestContext.Current.CancellationToken);

        var doluSil = await admin.DeleteAsync($"/api/roller/{dolu.Id}", TestContext.Current.CancellationToken);
        var bosSil = await admin.DeleteAsync($"/api/roller/{bos.Id}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, doluSil.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, bosSil.StatusCode);
    }

    private async Task<(int Id, string Ad)> RolAcAsync(HttpClient admin)
    {
        var ad = $"Rol-{Guid.NewGuid():N}"[..20];
        var yanit = await admin.PostAsJsonAsync("/api/roller", new { ad, korumali = false, yetkiler = Array.Empty<string>() }, TestContext.Current.CancellationToken);
        return ((await yanit.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken)).GetProperty("id").GetInt32(), ad);
    }

    private async Task<(int Id, HttpClient Client)> KullaniciAsync()
    {
        var eposta = $"kullanici-{Guid.NewGuid():N}@izintakip.test";
        var id = await TestAuth.KullaniciOlusturAsync(factory, eposta, SistemRolleri.Calisan);
        return (id, await TestAuth.OturumluClientAsync(factory, eposta));
    }

    private Task<HttpClient> SuperAdminAsync() =>
        TestAuth.OturumluClientAsync(factory, ApiFactory.SuperAdminEposta, ApiFactory.SuperAdminParola);
}
