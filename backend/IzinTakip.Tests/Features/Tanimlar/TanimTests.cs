using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using IzinTakip.Domain;

namespace IzinTakip.Tests.Features.Tanimlar;

public sealed class TanimTests(VeritabanliApiFactory factory) : IClassFixture<VeritabanliApiFactory>
{
    [Fact]
    public async Task IzinTurleri_BaslangicVerisi_YediTurVeSureMetniylaGelir()
    {
        var client = await SuperAdminAsync();

        var turler = await client.GetFromJsonAsync<JsonElement>("/api/izin-turleri", TestContext.Current.CancellationToken);
        var adlar = turler.EnumerateArray().Select(t => t.GetProperty("ad").GetString()).ToList();
        var evlilik = turler.EnumerateArray().Single(t => t.GetProperty("ad").GetString() == "Evlilik");

        Assert.Superset(new HashSet<string?>(["Yıllık", "Saatlik İzin", "Evlilik", "Ölüm", "Babalık", "Rapor", "Ücretsiz"]), adlar.ToHashSet());
        Assert.Equal(1440, evlilik.GetProperty("talepBasinaEnFazlaDakika").GetInt32());
        Assert.Equal("3 gün", evlilik.GetProperty("talepBasinaEnFazlaMetni").GetString());
    }

    [Fact]
    public async Task IzinTuru_TakvimGunuVeSaatlik_422VeKuralMesajiDoner()
    {
        var client = await SuperAdminAsync();

        var yanit = await client.PostAsJsonAsync("/api/izin-turleri", IzinTuru("Hatalı", "TakvimGunu", saatlik: true), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, yanit.StatusCode);
        Assert.Equal("Takvim günü sayan izin türü yalnız tam gün alınabilir.", await DetayAsync(yanit));
    }

    [Fact]
    public async Task IzinTuru_Olustur_IkIcin201CalisanIcin403()
    {
        await TestKimlik.KullaniciOlusturAsync(factory, "tanim-ik@izintakip.test", SistemRolleri.IK);
        await TestKimlik.KullaniciOlusturAsync(factory, "tanim-calisan@izintakip.test", SistemRolleri.Calisan);
        var ik = await TestKimlik.OturumluClientAsync(factory, "tanim-ik@izintakip.test");
        var calisan = await TestKimlik.OturumluClientAsync(factory, "tanim-calisan@izintakip.test");

        var ikYaniti = await ik.PostAsJsonAsync("/api/izin-turleri", IzinTuru("Mazeret", "IsGunu", saatlik: true), TestContext.Current.CancellationToken);
        var calisanYaniti = await calisan.PostAsJsonAsync("/api/izin-turleri", IzinTuru("Mazeret 2", "IsGunu", saatlik: true), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, ikYaniti.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, calisanYaniti.StatusCode);
    }

    [Fact]
    public async Task ResmiTatil_AyniTariheIkinciKayit_422Doner()
    {
        var client = await SuperAdminAsync();
        var tatil = new { tarih = new DateOnly(2030, 1, 1), ad = "Yılbaşı", yarimGun = false };

        var ilk = await client.PostAsJsonAsync("/api/resmi-tatiller", tatil, TestContext.Current.CancellationToken);
        var ikinci = await client.PostAsJsonAsync("/api/resmi-tatiller", tatil, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, ilk.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, ikinci.StatusCode);
        Assert.Equal("Bu tarihte zaten bir resmi tatil kayıtlı.", await DetayAsync(ikinci));
    }

    [Fact]
    public async Task Ayarlar_Guncelle_GunlukCalismaDakikasiniYenidenHesaplar()
    {
        var client = await SuperAdminAsync();

        var yanit = await client.PutAsJsonAsync("/api/ayarlar", Ayarlar("08:30", "17:30", "12:00", "13:00"), TestContext.Current.CancellationToken);
        var govde = await yanit.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        await client.PutAsJsonAsync("/api/ayarlar", Ayarlar("09:00", "18:00", "12:30", "13:30"), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, yanit.StatusCode);
        Assert.Equal(480, govde.GetProperty("gunlukCalismaDakikasi").GetInt32());
        Assert.Equal(["Pazartesi", "Sali", "Carsamba", "Persembe", "Cuma"], govde.GetProperty("calismaGunleri").EnumerateArray().Select(g => g.GetString()));
    }

    [Fact]
    public async Task Ayarlar_OgleArasiMesaiDisinda_422Doner()
    {
        var client = await SuperAdminAsync();

        var yanit = await client.PutAsJsonAsync("/api/ayarlar", Ayarlar("09:00", "18:00", "08:00", "09:30"), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, yanit.StatusCode);
    }

    [Fact]
    public async Task HakKurallari_BaslangicVerisi_BesKuralGelir()
    {
        var client = await SuperAdminAsync();

        var kurallar = await client.GetFromJsonAsync<JsonElement>("/api/hak-kurallari", TestContext.Current.CancellationToken);

        Assert.Equal(5, kurallar.GetArrayLength());
        Assert.Contains(kurallar.EnumerateArray(), k => k.GetProperty("minYas").ValueKind == JsonValueKind.Number && k.GetProperty("gun").GetInt32() == 20);
    }

    [Fact]
    public async Task Departman_AyniAdlaIkinciKayit_422Doner()
    {
        var client = await SuperAdminAsync();
        await TestVeri.DepartmanEkleAsync(client, "Muhasebe");

        var yanit = await client.PostAsJsonAsync("/api/departmanlar", new { ad = "Muhasebe" }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, yanit.StatusCode);
        Assert.Equal("Bu adla bir departman zaten var.", await DetayAsync(yanit));
    }

    [Fact]
    public async Task Departman_AktifCalisaniVarken_Pasiflestirilemez()
    {
        var client = await SuperAdminAsync();
        var departmanId = await TestVeri.DepartmanEkleAsync(client, "Lojistik");
        await TestVeri.CalisanEkleAsync(client, "Kerem", departmanId);

        var yanit = await client.PutAsJsonAsync($"/api/departmanlar/{departmanId}/aktiflik", new { aktif = false }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, yanit.StatusCode);
    }

    private Task<HttpClient> SuperAdminAsync() =>
        TestKimlik.OturumluClientAsync(factory, ApiFactory.SuperAdminEposta, ApiFactory.SuperAdminParola);

    private static object IzinTuru(string ad, string gunHesabi, bool saatlik) => new
    {
        ad,
        bakiyedenDuser = false,
        belgeZorunlu = false,
        gunHesabi,
        tamGunAlinir = true,
        yarimGunAlinir = false,
        saatlikAlinir = saatlik,
        talepBasinaEnFazlaDakika = (int?)null,
        yillikEnFazlaDakika = (int?)null
    };

    private static object Ayarlar(string mesaiBaslangic, string mesaiBitis, string ogleBaslangic, string ogleBitis) => new
    {
        calismaGunleri = new[] { "Pazartesi", "Sali", "Carsamba", "Persembe", "Cuma" },
        mesaiBaslangic,
        mesaiBitis,
        ogleArasiBaslangic = ogleBaslangic,
        ogleArasiBitis = ogleBitis,
        saatlikIzinAdimDakika = 30,
        hatirlatmaGunSayisi = 2
    };

    private static async Task<string?> DetayAsync(HttpResponseMessage yanit) =>
        (await yanit.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken)).GetProperty("detail").GetString();
}
