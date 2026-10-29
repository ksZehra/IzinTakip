using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using IzinTakip.Domain;

namespace IzinTakip.Tests.Features.IzinTalepleri;

public sealed class IzinOnayTests(DatabaseApiFactory factory) : IClassFixture<DatabaseApiFactory>
{
    private const int GunlukDakika = 480;
    private static readonly DateOnly Pazartesi = TestData.SonrakiYilPazartesi();

    [Fact]
    public async Task Olustur_CalisanTalebi_YoneticiyeBildirimVeEpostaGider()
    {
        var ekip = await EkipKurAsync();

        var talepId = await TalepAcAsync(ekip.Calisan);
        var bildirim = (await BildirimlerAsync(ekip.Yonetici)).GetProperty("items")[0];
        var eposta = await factory.EmailSender.BekleAsync(ekip.YoneticiEposta, "Onayınızı bekleyen izin talebi");

        Assert.Equal(1, await OkunmamisAsync(ekip.Yonetici));
        Assert.Equal(0, await OkunmamisAsync(ekip.Calisan));
        Assert.Equal("Onayınızı bekleyen izin talebi", bildirim.GetProperty("baslik").GetString());
        Assert.StartsWith("Çalışan Test izin talebi açtı. Yıllık:", bildirim.GetProperty("metin").GetString());
        Assert.Equal($"/izin-talepleri/{talepId}", bildirim.GetProperty("baglanti").GetString());
        Assert.Contains($"/izin-talepleri/{talepId}", eposta.HtmlGovde);
    }

    [Fact]
    public async Task Olustur_YoneticiEkibiAdina_TalepSahibineBildirimGiderYoneticiyeGitmez()
    {
        var ekip = await EkipKurAsync();
        var yillik = await TestData.IzinTuruIdAsync(ekip.Yonetici, "Yıllık");

        var yanit = await ekip.Yonetici.PostAsJsonAsync(
            "/api/izin-talepleri",
            new { calisanId = ekip.CalisanId, izinTuruId = yillik, sureTipi = "TamGun", tarih = Pazartesi },
            TestContext.Current.CancellationToken);
        var bildirim = (await BildirimlerAsync(ekip.Calisan)).GetProperty("items")[0];
        await factory.EmailSender.BekleAsync(ekip.CalisanEposta, "Adınıza izin talebi açıldı");

        Assert.Equal(HttpStatusCode.Created, yanit.StatusCode);
        Assert.Equal("Adınıza izin talebi açıldı", bildirim.GetProperty("baslik").GetString());
        Assert.Equal(0, await OkunmamisAsync(ekip.Yonetici));
    }

    [Fact]
    public async Task Onayla_YoneticiEkibininTalebini_OnaylanirVeSahibineBildirimGider()
    {
        var ekip = await EkipKurAsync();
        var talepId = await TalepAcAsync(ekip.Calisan);

        var yanit = await KararAsync(ekip.Yonetici, talepId, "onayla", new { not = "İyi tatiller" });
        var talep = await yanit.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        var bildirim = (await BildirimlerAsync(ekip.Calisan)).GetProperty("items")[0];

        Assert.Equal(HttpStatusCode.OK, yanit.StatusCode);
        Assert.Equal("Onaylandi", talep.GetProperty("durum").GetString());
        Assert.Equal("İyi tatiller", talep.GetProperty("kararNotu").GetString());
        Assert.Equal("Yönetici Test", talep.GetProperty("kararVerenAdi").GetString());
        Assert.Equal("Çalışan Test", talep.GetProperty("olusturanAdi").GetString());
        Assert.Equal("İzin talebiniz onaylandı", bildirim.GetProperty("baslik").GetString());
        Assert.EndsWith("Not: İyi tatiller", bildirim.GetProperty("metin").GetString());
        await factory.EmailSender.BekleAsync(ekip.CalisanEposta, "İzin talebiniz onaylandı");
    }

    [Fact]
    public async Task Onayla_Calisan_403Doner()
    {
        var ekip = await EkipKurAsync();
        var talepId = await TalepAcAsync(ekip.Calisan);

        var yanit = await KararAsync(ekip.Calisan, talepId, "onayla", new { });

        Assert.Equal(HttpStatusCode.Forbidden, yanit.StatusCode);
    }

    [Fact]
    public async Task Onayla_YoneticiKendiTalebini_422VeTalepIkYeDuser()
    {
        var ekip = await EkipKurAsync();
        var ikEposta = $"ik-{Guid.NewGuid():N}@izintakip.test";
        await TestAuth.KullaniciOlusturAsync(factory, ikEposta, SistemRolleri.IK);
        var ik = await TestAuth.OturumluClientAsync(factory, ikEposta);
        var talepId = await TalepAcAsync(ekip.Yonetici);

        var kendisi = await KararAsync(ekip.Yonetici, talepId, "onayla", new { });
        var ikYaniti = await KararAsync(ik, talepId, "onayla", new { });

        Assert.Equal(1, await OkunmamisAsync(ik));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, kendisi.StatusCode);
        Assert.Equal("Kendi izin talebinizi onaylayamazsınız.", await DetayAsync(kendisi));
        Assert.Equal(HttpStatusCode.OK, ikYaniti.StatusCode);
    }

    [Fact]
    public async Task Onayla_SuperAdminKendiTalebini_Onaylanir()
    {
        var admin = await SuperAdminAsync();
        var departmanId = await TestData.DepartmanEkleAsync(admin, $"Yonetim-{Guid.NewGuid():N}");
        var calisanId = await TestData.CalisanEkleAsync(admin, "Admin", departmanId);
        await TestData.HakEkleAsync(admin, calisanId, 14 * GunlukDakika);
        var eposta = $"superadmin-{calisanId}@izintakip.test";
        await TestAuth.KullaniciOlusturAsync(factory, eposta, SistemRolleri.SuperAdmin, calisanId: calisanId);
        var superAdmin = await TestAuth.OturumluClientAsync(factory, eposta);
        var talepId = await TalepAcAsync(superAdmin);

        var yanit = await KararAsync(superAdmin, talepId, "onayla", new { });

        Assert.Equal(HttpStatusCode.OK, yanit.StatusCode);
    }

    [Fact]
    public async Task Onayla_KararVerilmisTalep_422Doner()
    {
        var ekip = await EkipKurAsync();
        var talepId = await TalepAcAsync(ekip.Calisan);
        await KararAsync(ekip.Yonetici, talepId, "onayla", new { });

        var ikinci = await KararAsync(ekip.Yonetici, talepId, "reddet", new { gerekce = "Vazgeçtim" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, ikinci.StatusCode);
        Assert.Equal("Bu talep artık beklemede değil.", await DetayAsync(ikinci));
    }

    [Fact]
    public async Task Onayla_EkipDisindakiTalep_404Doner()
    {
        var ekip = await EkipKurAsync();
        var digerEkip = await EkipKurAsync();
        var talepId = await TalepAcAsync(digerEkip.Calisan);

        var yanit = await KararAsync(ekip.Yonetici, talepId, "onayla", new { });

        Assert.Equal(HttpStatusCode.NotFound, yanit.StatusCode);
    }

    [Fact]
    public async Task Reddet_GerekceBos_400AlanHatasiDoner()
    {
        var ekip = await EkipKurAsync();
        var talepId = await TalepAcAsync(ekip.Calisan);

        var yanit = await KararAsync(ekip.Yonetici, talepId, "reddet", new { gerekce = "" });
        var govde = await yanit.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, yanit.StatusCode);
        Assert.Equal("Red gerekçesi zorunludur.", govde.GetProperty("errors").GetProperty("gerekce")[0].GetString());
    }

    [Fact]
    public async Task Reddet_Yonetici_ReddedilirVeBakiyeSerbestKalir()
    {
        var ekip = await EkipKurAsync();
        var talepId = await TalepAcAsync(ekip.Calisan);

        var yanit = await KararAsync(ekip.Yonetici, talepId, "reddet", new { gerekce = "Ekip o hafta yoğun" });
        var talep = await yanit.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        var bakiye = await ekip.Calisan.GetFromJsonAsync<JsonElement>($"/api/calisanlar/{ekip.CalisanId}/bakiye", TestContext.Current.CancellationToken);

        Assert.Equal("Reddedildi", talep.GetProperty("durum").GetString());
        Assert.Equal("Ekip o hafta yoğun", talep.GetProperty("kararNotu").GetString());
        Assert.Equal(0, bakiye.GetProperty("bekleyenDakika").GetInt32());
        await factory.EmailSender.BekleAsync(ekip.CalisanEposta, "İzin talebiniz reddedildi");
    }

    [Fact]
    public async Task IptalEt_CalisanKendiTalebini_IptalEdilirVeYoneticiyeBildirimGider()
    {
        var ekip = await EkipKurAsync();
        var talepId = await TalepAcAsync(ekip.Calisan);

        var yanit = await KararAsync(ekip.Calisan, talepId, "iptal", new { });
        var talep = await yanit.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        var bildirim = (await BildirimlerAsync(ekip.Yonetici)).GetProperty("items")[0];

        Assert.Equal("IptalEdildi", talep.GetProperty("durum").GetString());
        Assert.Equal(2, await OkunmamisAsync(ekip.Yonetici));
        Assert.Equal("İzin talebi iptal edildi", bildirim.GetProperty("baslik").GetString());
        Assert.StartsWith("Çalışan Test izin talebini iptal etti.", bildirim.GetProperty("metin").GetString());
    }

    [Fact]
    public async Task IptalEt_YoneticiEkibininOnayliTalebini_SahibineBildirimGider()
    {
        var ekip = await EkipKurAsync();
        var talepId = await TalepAcAsync(ekip.Calisan);
        await KararAsync(ekip.Yonetici, talepId, "onayla", new { });

        var yanit = await KararAsync(ekip.Yonetici, talepId, "iptal", new { not = "Proje teslimi öne alındı" });
        var bildirim = (await BildirimlerAsync(ekip.Calisan)).GetProperty("items")[0];

        Assert.Equal(HttpStatusCode.OK, yanit.StatusCode);
        Assert.Equal("İzin talebiniz iptal edildi", bildirim.GetProperty("baslik").GetString());
        Assert.EndsWith("Not: Proje teslimi öne alındı", bildirim.GetProperty("metin").GetString());
    }

    [Fact]
    public async Task OnayVeRed_AyniAnda_YalnizBiriUygulanir()
    {
        var ekip = await EkipKurAsync();
        var talepId = await TalepAcAsync(ekip.Calisan);
        var ikEposta = $"ik-{Guid.NewGuid():N}@izintakip.test";
        await TestAuth.KullaniciOlusturAsync(factory, ikEposta, SistemRolleri.IK);
        var ik = await TestAuth.OturumluClientAsync(factory, ikEposta);

        var onayGorevi = KararAsync(ekip.Yonetici, talepId, "onayla", new { });
        var redGorevi = KararAsync(ik, talepId, "reddet", new { gerekce = "Ekip planı uygun değil" });
        await Task.WhenAll(onayGorevi, redGorevi);
        var (onay, red) = (await onayGorevi, await redGorevi);
        var talep = await ekip.Calisan.GetFromJsonAsync<JsonElement>($"/api/izin-talepleri/{talepId}", TestContext.Current.CancellationToken);

        Assert.Single(new[] { onay, red }, y => y.IsSuccessStatusCode);
        Assert.Contains((onay.IsSuccessStatusCode ? red : onay).StatusCode, new[] { HttpStatusCode.Conflict, HttpStatusCode.UnprocessableEntity });
        Assert.Equal(onay.IsSuccessStatusCode ? "Onaylandi" : "Reddedildi", talep.GetProperty("durum").GetString());
    }

    private async Task<Ekip> EkipKurAsync()
    {
        var admin = await SuperAdminAsync();
        var departmanId = await TestData.DepartmanEkleAsync(admin, $"Ekip-{Guid.NewGuid():N}");
        var yoneticiId = await TestData.CalisanEkleAsync(admin, "Yönetici", departmanId);
        await TestData.DepartmanYoneticisiAtaAsync(admin, departmanId, $"Ekip-{yoneticiId}", yoneticiId);
        var calisanId = await TestData.CalisanEkleAsync(admin, "Çalışan", departmanId);
        await TestData.HakEkleAsync(admin, yoneticiId, 14 * GunlukDakika);
        await TestData.HakEkleAsync(admin, calisanId, 14 * GunlukDakika);
        var yoneticiEposta = $"yonetici-{yoneticiId}@izintakip.test";
        var calisanEposta = $"calisan-{calisanId}@izintakip.test";
        await TestAuth.KullaniciOlusturAsync(factory, yoneticiEposta, SistemRolleri.Yonetici, calisanId: yoneticiId);
        await TestAuth.KullaniciOlusturAsync(factory, calisanEposta, SistemRolleri.Calisan, calisanId: calisanId);
        return new Ekip(
            calisanId,
            calisanEposta,
            await TestAuth.OturumluClientAsync(factory, calisanEposta),
            yoneticiEposta,
            await TestAuth.OturumluClientAsync(factory, yoneticiEposta));
    }

    private Task<HttpClient> SuperAdminAsync() =>
        TestAuth.OturumluClientAsync(factory, ApiFactory.SuperAdminEposta, ApiFactory.SuperAdminParola);

    private static async Task<int> TalepAcAsync(HttpClient client)
    {
        var yillik = await TestData.IzinTuruIdAsync(client, "Yıllık");
        var yanit = await client.PostAsJsonAsync(
            "/api/izin-talepleri",
            new { izinTuruId = yillik, sureTipi = "TamGun", tarih = Pazartesi, bitisTarihi = Pazartesi.AddDays(1) },
            TestContext.Current.CancellationToken);
        var govde = await yanit.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.True(yanit.StatusCode == HttpStatusCode.Created, govde);
        return JsonDocument.Parse(govde).RootElement.GetProperty("id").GetInt32();
    }

    private static Task<HttpResponseMessage> KararAsync(HttpClient client, int talepId, string islem, object govde) =>
        client.PostAsJsonAsync($"/api/izin-talepleri/{talepId}/{islem}", govde, TestContext.Current.CancellationToken);

    private static Task<JsonElement> BildirimlerAsync(HttpClient client) =>
        client.GetFromJsonAsync<JsonElement>("/api/bildirimler", TestContext.Current.CancellationToken);

    private static async Task<int> OkunmamisAsync(HttpClient client) =>
        (await client.GetFromJsonAsync<JsonElement>("/api/bildirimler/sayi", TestContext.Current.CancellationToken)).GetProperty("okunmamis").GetInt32();

    private static async Task<string?> DetayAsync(HttpResponseMessage yanit) =>
        (await yanit.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken)).GetProperty("detail").GetString();

    private sealed record Ekip(int CalisanId, string CalisanEposta, HttpClient Calisan, string YoneticiEposta, HttpClient Yonetici);
}
