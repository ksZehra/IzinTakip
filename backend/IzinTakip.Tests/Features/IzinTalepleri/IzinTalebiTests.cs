using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using IzinTakip.Domain;

namespace IzinTakip.Tests.Features.IzinTalepleri;

public sealed class IzinTalebiTests(VeritabanliApiFactory factory) : IClassFixture<VeritabanliApiFactory>
{
    private const int GunlukDakika = 480;
    private static readonly DateOnly Pazartesi = TestVeri.SonrakiYilPazartesi();

    [Fact]
    public async Task Olustur_KendiAdinaTamGun_201BeklemedeVeBakiyedenDuser()
    {
        var (calisanId, client) = await CalisanKurAsync();
        var yillik = await TestVeri.IzinTuruIdAsync(client, "Yıllık");

        var yanit = await TalepAcAsync(client, new { izinTuruId = yillik, sureTipi = "TamGun", tarih = Pazartesi, bitisTarihi = Pazartesi.AddDays(2) });
        var talep = await yanit.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        var bakiye = await client.GetFromJsonAsync<JsonElement>($"/api/calisanlar/{calisanId}/bakiye", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, yanit.StatusCode);
        Assert.Equal("Beklemede", talep.GetProperty("durum").GetString());
        Assert.Equal("3 gün", talep.GetProperty("sureMetni").GetString());
        Assert.Equal(3 * GunlukDakika, bakiye.GetProperty("bekleyenDakika").GetInt32());
        Assert.Equal("11 gün", bakiye.GetProperty("kullanilabilirMetni").GetString());
    }

    [Fact]
    public async Task Olustur_YarimGunVeSaatlik_DogruSureylerOlusur()
    {
        var (_, client) = await CalisanKurAsync();
        var yillik = await TestVeri.IzinTuruIdAsync(client, "Yıllık");
        var saatlik = await TestVeri.IzinTuruIdAsync(client, "Saatlik İzin");

        var yarim = await TalepAcAsync(client, new { izinTuruId = yillik, sureTipi = "YarimGun", tarih = Pazartesi, gunYarisi = "Sabah" });
        var saat = await TalepAcAsync(client, new { izinTuruId = saatlik, sureTipi = "Saatlik", tarih = Pazartesi, baslangicSaati = "14:00", bitisSaati = "16:00" });

        Assert.Equal(HttpStatusCode.Created, yarim.StatusCode);
        Assert.Equal(240, (await yarim.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken)).GetProperty("dakika").GetInt32());
        Assert.Equal(HttpStatusCode.Created, saat.StatusCode);
        Assert.Equal("2 saat", (await saat.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken)).GetProperty("sureMetni").GetString());
    }

    [Fact]
    public async Task Olustur_BakiyeYetersiz_422VeAcikMesajDoner()
    {
        var (_, client) = await CalisanKurAsync();
        var yillik = await TestVeri.IzinTuruIdAsync(client, "Yıllık");

        var yanit = await TalepAcAsync(client, new { izinTuruId = yillik, sureTipi = "TamGun", tarih = Pazartesi, bitisTarihi = Pazartesi.AddDays(18) });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, yanit.StatusCode);
        Assert.Equal("Bakiye yetersiz: kullanılabilir 14 gün, talep 15 gün.", await DetayAsync(yanit));
    }

    [Fact]
    public async Task Olustur_CakisanIkinciTalep_422Doner()
    {
        var (_, client) = await CalisanKurAsync();
        var yillik = await TestVeri.IzinTuruIdAsync(client, "Yıllık");

        await TalepAcAsync(client, new { izinTuruId = yillik, sureTipi = "TamGun", tarih = Pazartesi, bitisTarihi = Pazartesi.AddDays(1) });
        var ikinci = await TalepAcAsync(client, new { izinTuruId = yillik, sureTipi = "YarimGun", tarih = Pazartesi.AddDays(1), gunYarisi = "OgledenSonra" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, ikinci.StatusCode);
        Assert.Equal("Bu tarihlerle çakışan, beklemede ya da onaylanmış başka bir izin talebi var.", await DetayAsync(ikinci));
    }

    [Fact]
    public async Task Olustur_GecmisTarih_CalisanaKapaliIkeAcik()
    {
        var (calisanId, calisan) = await CalisanKurAsync();
        var yillik = await TestVeri.IzinTuruIdAsync(calisan, "Yıllık");
        var gecmis = TestVeri.GecenYilPazartesi();

        var calisanYaniti = await TalepAcAsync(calisan, new { izinTuruId = yillik, sureTipi = "TamGun", tarih = gecmis });
        var ik = await IkClientAsync();
        var ikYaniti = await TalepAcAsync(ik, new { calisanId, izinTuruId = yillik, sureTipi = "TamGun", tarih = gecmis });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, calisanYaniti.StatusCode);
        Assert.Equal("Geçmiş tarihli izin talebi açılamaz.", await DetayAsync(calisanYaniti));
        Assert.Equal(HttpStatusCode.Created, ikYaniti.StatusCode);
    }

    [Fact]
    public async Task Olustur_CalisanBaskasiAdina_403Doner()
    {
        var (_, client) = await CalisanKurAsync();
        var (digerId, _) = await CalisanKurAsync();
        var yillik = await TestVeri.IzinTuruIdAsync(client, "Yıllık");

        var yanit = await TalepAcAsync(client, new { calisanId = digerId, izinTuruId = yillik, sureTipi = "TamGun", tarih = Pazartesi });

        Assert.Equal(HttpStatusCode.Forbidden, yanit.StatusCode);
        Assert.Equal("Başkası adına izin talebi açma yetkiniz yok.", await DetayAsync(yanit));
    }

    [Fact]
    public async Task Olustur_Yonetici_EkibiAdinaAcarEkipDisinaAcamaz()
    {
        var admin = await SuperAdminAsync();
        var ekip = await TestVeri.DepartmanEkleAsync(admin, $"Ekip-{Guid.NewGuid():N}");
        var yoneticiId = await TestVeri.CalisanEkleAsync(admin, "Yönetici", ekip);
        await TestVeri.DepartmanYoneticisiAtaAsync(admin, ekip, $"Ekip-{yoneticiId}", yoneticiId);
        var ekiptenId = await TestVeri.CalisanEkleAsync(admin, "Ekipten", ekip);
        await TestVeri.HakEkleAsync(admin, ekiptenId, 14 * GunlukDakika);
        var (disaridanId, _) = await CalisanKurAsync();
        var eposta = $"yonetici-{yoneticiId}@izintakip.test";
        await TestKimlik.KullaniciOlusturAsync(factory, eposta, SistemRolleri.Yonetici, calisanId: yoneticiId);
        var yonetici = await TestKimlik.OturumluClientAsync(factory, eposta);
        var yillik = await TestVeri.IzinTuruIdAsync(yonetici, "Yıllık");

        var ekibine = await TalepAcAsync(yonetici, new { calisanId = ekiptenId, izinTuruId = yillik, sureTipi = "TamGun", tarih = Pazartesi });
        var disariya = await TalepAcAsync(yonetici, new { calisanId = disaridanId, izinTuruId = yillik, sureTipi = "TamGun", tarih = Pazartesi });

        Assert.Equal(HttpStatusCode.Created, ekibine.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, disariya.StatusCode);
    }

    [Fact]
    public async Task Liste_Calisan_YalnizKendiTaleplerini_Gorur()
    {
        var (_, birinci) = await CalisanKurAsync();
        var (_, ikinci) = await CalisanKurAsync();
        var yillik = await TestVeri.IzinTuruIdAsync(birinci, "Yıllık");
        await TalepAcAsync(birinci, new { izinTuruId = yillik, sureTipi = "TamGun", tarih = Pazartesi });
        await TalepAcAsync(ikinci, new { izinTuruId = yillik, sureTipi = "TamGun", tarih = Pazartesi });

        var liste = await birinci.GetFromJsonAsync<JsonElement>("/api/izin-talepleri", TestContext.Current.CancellationToken);

        Assert.Equal(1, liste.GetProperty("totalCount").GetInt32());
        Assert.Equal("1 gün", liste.GetProperty("items")[0].GetProperty("sureMetni").GetString());
    }

    [Fact]
    public async Task Detay_BaskasininTalebi_404Doner()
    {
        var (_, birinci) = await CalisanKurAsync();
        var (_, ikinci) = await CalisanKurAsync();
        var yillik = await TestVeri.IzinTuruIdAsync(birinci, "Yıllık");
        var talep = await (await TalepAcAsync(birinci, new { izinTuruId = yillik, sureTipi = "TamGun", tarih = Pazartesi }))
            .Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);

        var yanit = await ikinci.GetAsync($"/api/izin-talepleri/{talep.GetProperty("id").GetInt32()}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, yanit.StatusCode);
    }

    [Fact]
    public async Task SureHesapla_SaatlikOgleArasiniKapsayan_IkiSaatDoner()
    {
        var (_, client) = await CalisanKurAsync();
        var saatlik = await TestVeri.IzinTuruIdAsync(client, "Saatlik İzin");

        var yanit = await client.PostAsJsonAsync(
            "/api/izin-talepleri/sure-hesapla",
            new { izinTuruId = saatlik, sureTipi = "Saatlik", tarih = Pazartesi, baslangicSaati = "11:00", bitisSaati = "14:00" },
            TestContext.Current.CancellationToken);
        var govde = await yanit.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, yanit.StatusCode);
        Assert.Equal(120, govde.GetProperty("dakika").GetInt32());
        Assert.Equal("2 saat", govde.GetProperty("sureMetni").GetString());
    }

    [Fact]
    public async Task Olustur_SaatlikteBitisSaatiYok_400AlanHatasiDoner()
    {
        var (_, client) = await CalisanKurAsync();
        var saatlik = await TestVeri.IzinTuruIdAsync(client, "Saatlik İzin");

        var yanit = await TalepAcAsync(client, new { izinTuruId = saatlik, sureTipi = "Saatlik", tarih = Pazartesi, baslangicSaati = "10:00" });
        var govde = await yanit.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, yanit.StatusCode);
        Assert.Equal("Bitiş saati zorunludur.", govde.GetProperty("errors").GetProperty("bitisSaati")[0].GetString());
    }

    private async Task<(int CalisanId, HttpClient Client)> CalisanKurAsync()
    {
        var admin = await SuperAdminAsync();
        var departmanId = await TestVeri.DepartmanEkleAsync(admin, $"Departman-{Guid.NewGuid():N}");
        var calisanId = await TestVeri.CalisanEkleAsync(admin, "Çalışan", departmanId);
        await TestVeri.HakEkleAsync(admin, calisanId, 14 * GunlukDakika);
        var eposta = $"calisan-{calisanId}@izintakip.test";
        await TestKimlik.KullaniciOlusturAsync(factory, eposta, SistemRolleri.Calisan, calisanId: calisanId);
        return (calisanId, await TestKimlik.OturumluClientAsync(factory, eposta));
    }

    private async Task<HttpClient> IkClientAsync()
    {
        var eposta = $"ik-{Guid.NewGuid():N}@izintakip.test";
        await TestKimlik.KullaniciOlusturAsync(factory, eposta, SistemRolleri.IK);
        return await TestKimlik.OturumluClientAsync(factory, eposta);
    }

    private Task<HttpClient> SuperAdminAsync() =>
        TestKimlik.OturumluClientAsync(factory, ApiFactory.SuperAdminEposta, ApiFactory.SuperAdminParola);

    private static Task<HttpResponseMessage> TalepAcAsync(HttpClient client, object talep) =>
        client.PostAsJsonAsync("/api/izin-talepleri", talep, TestContext.Current.CancellationToken);

    private static async Task<string?> DetayAsync(HttpResponseMessage yanit) =>
        (await yanit.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken)).GetProperty("detail").GetString();
}
