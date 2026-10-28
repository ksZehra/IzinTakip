using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using IzinTakip.Api.BackgroundServices;
using IzinTakip.Api.Common;
using IzinTakip.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IzinTakip.Tests.BackgroundServices;

public sealed class ArkaPlanGorevleriTests(VeritabanliApiFactory factory) : IClassFixture<VeritabanliApiFactory>
{
    [Fact]
    public async Task YillikHak_YildonumuGelenCalisana_KuralHakkiBirKezYazilir()
    {
        var bugun = factory.Services.GetRequiredService<SirketSaati>().Bugun();
        var admin = await SuperAdminAsync();
        var departmanId = await TestVeri.DepartmanEkleAsync(admin, $"Departman-{Guid.NewGuid():N}");
        var calisanId = await TestVeri.CalisanEkleAsync(admin, "Kıdemli", departmanId, iseGirisTarihi: bugun.AddYears(-3).AddDays(10));
        var gorev = Gorev<YillikHakServisi>();

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
        var departmanId = await TestVeri.DepartmanEkleAsync(admin, $"Departman-{Guid.NewGuid():N}");
        var iseGiris = bugun.AddYears(-3).AddDays(10);
        var calisanId = await TestVeri.CalisanEkleAsync(admin, "Düzeltilen", departmanId, iseGirisTarihi: iseGiris);
        var gorev = Gorev<YillikHakServisi>();
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
        var departmanId = await TestVeri.DepartmanEkleAsync(admin, $"Ekip-{Guid.NewGuid():N}");
        var yoneticiId = await TestVeri.CalisanEkleAsync(admin, "Yönetici", departmanId);
        await TestVeri.DepartmanYoneticisiAtaAsync(admin, departmanId, $"Ekip-{yoneticiId}", yoneticiId);
        var calisanId = await TestVeri.CalisanEkleAsync(admin, "Bekleyen", departmanId);
        await TestVeri.HakEkleAsync(admin, calisanId, 14 * 480);
        var yoneticiEposta = $"yonetici-{yoneticiId}@izintakip.test";
        await TestKimlik.KullaniciOlusturAsync(factory, yoneticiEposta, SistemRolleri.Yonetici, calisanId: yoneticiId);
        var yonetici = await TestKimlik.OturumluClientAsync(factory, yoneticiEposta);
        var yillik = await TestVeri.IzinTuruIdAsync(admin, "Yıllık");
        var talep = await admin.PostAsJsonAsync(
            "/api/izin-talepleri",
            new { calisanId, izinTuruId = yillik, sureTipi = "TamGun", tarih = TestVeri.SonrakiYilPazartesi() },
            TestContext.Current.CancellationToken);
        talep.EnsureSuccessStatusCode();
        var gorev = Gorev<HatirlatmaServisi>();

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

    private T Gorev<T>() where T : GunlukGorev =>
        factory.Services.GetServices<IHostedService>().OfType<T>().Single();

    private Task<HttpClient> SuperAdminAsync() =>
        TestKimlik.OturumluClientAsync(factory, ApiFactory.SuperAdminEposta, ApiFactory.SuperAdminParola);

    private static Task<JsonElement> HaklarAsync(HttpClient client, int calisanId) =>
        client.GetFromJsonAsync<JsonElement>($"/api/calisanlar/{calisanId}/haklar", TestContext.Current.CancellationToken);

    private static async Task<int> OkunmamisAsync(HttpClient client) =>
        (await client.GetFromJsonAsync<JsonElement>("/api/bildirimler/sayi", TestContext.Current.CancellationToken)).GetProperty("okunmamis").GetInt32();
}
