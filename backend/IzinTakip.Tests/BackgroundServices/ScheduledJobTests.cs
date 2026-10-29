using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using IzinTakip.Api.BackgroundServices;
using IzinTakip.Api.Common;
using IzinTakip.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IzinTakip.Tests.BackgroundServices;

public sealed class ScheduledJobTests(DatabaseApiFactory factory) : IClassFixture<DatabaseApiFactory>
{
    [Fact]
    public async Task YillikHak_YildonumuGelenCalisana_KuralHakkiBirKezYazilir()
    {
        var bugun = factory.Services.GetRequiredService<SirketSaati>().Bugun();
        var admin = await SuperAdminAsync();
        var departmanId = await TestData.DepartmanEkleAsync(admin, $"Departman-{Guid.NewGuid():N}");
        var calisanId = await TestData.CalisanEkleAsync(admin, "Kıdemli", departmanId, iseGirisTarihi: bugun.AddYears(-3).AddDays(10));
        var gorev = Gorev<YillikHakJob>();

        await gorev.CalistirAsync(bugun.AddDays(9), TestContext.Current.CancellationToken);
        var oncesi = await HaklarAsync(admin, calisanId);
        await gorev.CalistirAsync(bugun.AddDays(15), TestContext.Current.CancellationToken);
        await gorev.CalistirAsync(bugun.AddDays(15), TestContext.Current.CancellationToken);
        var sonrasi = await HaklarAsync(admin, calisanId);

        Assert.Equal(0, oncesi.GetProperty("satirlar").GetArrayLength());
        var hak = Assert.Single(sonrasi.GetProperty("satirlar").EnumerateArray());
        Assert.Equal(14 * 480, hak.GetProperty("dakika").GetInt32());
        Assert.Equal("Kural", hak.GetProperty("kaynak").GetString());
        Assert.Equal("3. yıl hakkı (1–5 yıl)", hak.GetProperty("aciklama").GetString());
    }

    [Fact]
    public async Task YillikHak_IseGirisTarihiDuzeltilince_AyniYilTekrarYazilmaz()
    {
        var bugun = factory.Services.GetRequiredService<SirketSaati>().Bugun();
        var admin = await SuperAdminAsync();
        var departmanId = await TestData.DepartmanEkleAsync(admin, $"Departman-{Guid.NewGuid():N}");
        var iseGiris = bugun.AddYears(-3).AddDays(10);
        var calisanId = await TestData.CalisanEkleAsync(admin, "Düzeltilen", departmanId, iseGirisTarihi: iseGiris);
        var gorev = Gorev<YillikHakJob>();
        await gorev.CalistirAsync(bugun.AddDays(15), TestContext.Current.CancellationToken);

        var duzeltme = await admin.PutAsJsonAsync(
            $"/api/calisanlar/{calisanId}",
            new { ad = "Düzeltilen", soyad = "Test", departmanId, iseGirisTarihi = iseGiris.AddMonths(-1), dogumTarihi = new DateOnly(1990, 5, 15) },
            TestContext.Current.CancellationToken);
        await gorev.CalistirAsync(bugun.AddDays(15), TestContext.Current.CancellationToken);
        var haklar = await HaklarAsync(admin, calisanId);

        Assert.Equal(HttpStatusCode.OK, duzeltme.StatusCode);
        var hak = Assert.Single(haklar.GetProperty("satirlar").EnumerateArray());
        Assert.Equal(3, hak.GetProperty("kidemYili").GetInt32());
    }

    [Fact]
    public async Task Hatirlatma_GunSayisiniGecenTalep_OnayciyaGunBasinaBirKezGider()
    {
        var bugun = factory.Services.GetRequiredService<SirketSaati>().Bugun();
        var admin = await SuperAdminAsync();
        var departmanId = await TestData.DepartmanEkleAsync(admin, $"Ekip-{Guid.NewGuid():N}");
        var yoneticiId = await TestData.CalisanEkleAsync(admin, "Yönetici", departmanId);
        await TestData.DepartmanYoneticisiAtaAsync(admin, departmanId, $"Ekip-{yoneticiId}", yoneticiId);
        var calisanId = await TestData.CalisanEkleAsync(admin, "Bekleyen", departmanId);
        await TestData.HakEkleAsync(admin, calisanId, 14 * 480);
        var yoneticiEposta = $"yonetici-{yoneticiId}@izintakip.test";
        await TestAuth.KullaniciOlusturAsync(factory, yoneticiEposta, SistemRolleri.Yonetici, calisanId: yoneticiId);
        var yonetici = await TestAuth.OturumluClientAsync(factory, yoneticiEposta);
        var yillik = await TestData.IzinTuruIdAsync(admin, "Yıllık");
        var talep = await admin.PostAsJsonAsync(
            "/api/izin-talepleri",
            new { calisanId, izinTuruId = yillik, sureTipi = "TamGun", tarih = TestData.SonrakiYilPazartesi() },
            TestContext.Current.CancellationToken);
        talep.EnsureSuccessStatusCode();
        var gorev = Gorev<OnayHatirlatmaJob>();

        await gorev.CalistirAsync(bugun.AddDays(1), TestContext.Current.CancellationToken);
        var erken = await OkunmamisAsync(yonetici);
        await gorev.CalistirAsync(bugun.AddDays(2), TestContext.Current.CancellationToken);
        await gorev.CalistirAsync(bugun.AddDays(2), TestContext.Current.CancellationToken);
        var bildirimler = await yonetici.GetFromJsonAsync<JsonElement>("/api/bildirimler", TestContext.Current.CancellationToken);

        Assert.Equal(1, erken);
        Assert.Equal(2, bildirimler.GetProperty("totalCount").GetInt32());
        Assert.StartsWith(
            "Bekleyen Test adlı çalışanın izin talebi 2 gündür onay bekliyor.",
            bildirimler.GetProperty("items")[0].GetProperty("metin").GetString());
        await factory.EmailSender.BekleAsync(yoneticiEposta, "Hatırlatma: onayınızı bekleyen izin talebi");
    }

    [Fact]
    public async Task Hatirlatma_HerTalepKendiOnaycisinaGiderPasifCalisanAtlanir()
    {
        var bugun = factory.Services.GetRequiredService<SirketSaati>().Bugun();
        var admin = await SuperAdminAsync();
        var yillik = await TestData.IzinTuruIdAsync(admin, "Yıllık");
        var (birinciYonetici, birinciDepartman) = await EkipAsync(admin);
        var (ikinciYonetici, ikinciDepartman) = await EkipAsync(admin);
        await TalepliCalisanAsync(admin, "Aktif", birinciDepartman, yillik);
        var ayrilan = await TalepliCalisanAsync(admin, "Ayrılan", birinciDepartman, yillik);
        await TalepliCalisanAsync(admin, "Diğer", ikinciDepartman, yillik);
        (await admin.PutAsJsonAsync($"/api/calisanlar/{ayrilan}/aktiflik", new { aktif = false }, TestContext.Current.CancellationToken))
            .EnsureSuccessStatusCode();

        await Gorev<OnayHatirlatmaJob>().CalistirAsync(bugun.AddDays(2), TestContext.Current.CancellationToken);
        var birinci = await birinciYonetici.GetFromJsonAsync<JsonElement>("/api/bildirimler", TestContext.Current.CancellationToken);
        var ikinci = await ikinciYonetici.GetFromJsonAsync<JsonElement>("/api/bildirimler", TestContext.Current.CancellationToken);

        var birinciHatirlatmalar = Hatirlatmalar(birinci);
        Assert.Single(birinciHatirlatmalar);
        Assert.StartsWith("Aktif Test", birinciHatirlatmalar[0]);
        Assert.StartsWith("Diğer Test", Assert.Single(Hatirlatmalar(ikinci)));
    }

    private async Task<(HttpClient Yonetici, int DepartmanId)> EkipAsync(HttpClient admin)
    {
        var departmanId = await TestData.DepartmanEkleAsync(admin, $"Ekip-{Guid.NewGuid():N}");
        var yoneticiId = await TestData.CalisanEkleAsync(admin, "Yönetici", departmanId);
        await TestData.DepartmanYoneticisiAtaAsync(admin, departmanId, $"Ekip-{yoneticiId}", yoneticiId);
        var eposta = $"yonetici-{yoneticiId}@izintakip.test";
        await TestAuth.KullaniciOlusturAsync(factory, eposta, SistemRolleri.Yonetici, calisanId: yoneticiId);
        return (await TestAuth.OturumluClientAsync(factory, eposta), departmanId);
    }

    private static async Task<int> TalepliCalisanAsync(HttpClient admin, string ad, int departmanId, int izinTuruId)
    {
        var calisanId = await TestData.CalisanEkleAsync(admin, ad, departmanId);
        await TestData.HakEkleAsync(admin, calisanId, 14 * 480);
        (await admin.PostAsJsonAsync(
                "/api/izin-talepleri",
                new { calisanId, izinTuruId, sureTipi = "TamGun", tarih = TestData.SonrakiYilPazartesi() },
                TestContext.Current.CancellationToken))
            .EnsureSuccessStatusCode();
        return calisanId;
    }

    private static List<string> Hatirlatmalar(JsonElement bildirimler) =>
        bildirimler.GetProperty("items").EnumerateArray()
            .Where(b => b.GetProperty("baslik").GetString() == "Hatırlatma: onayınızı bekleyen izin talebi")
            .Select(b => b.GetProperty("metin").GetString()!)
            .ToList();

    private T Gorev<T>() where T : DailyJob =>
        factory.Services.GetServices<IHostedService>().OfType<T>().Single();

    private Task<HttpClient> SuperAdminAsync() =>
        TestAuth.OturumluClientAsync(factory, ApiFactory.SuperAdminEposta, ApiFactory.SuperAdminParola);

    private static Task<JsonElement> HaklarAsync(HttpClient client, int calisanId) =>
        client.GetFromJsonAsync<JsonElement>($"/api/calisanlar/{calisanId}/haklar", TestContext.Current.CancellationToken);

    private static async Task<int> OkunmamisAsync(HttpClient client) =>
        (await client.GetFromJsonAsync<JsonElement>("/api/bildirimler/sayi", TestContext.Current.CancellationToken)).GetProperty("okunmamis").GetInt32();
}
