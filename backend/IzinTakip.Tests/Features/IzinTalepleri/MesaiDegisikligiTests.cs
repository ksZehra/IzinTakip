using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace IzinTakip.Tests.Features.IzinTalepleri;

public sealed class MesaiDegisikligiTests(DatabaseApiFactory factory) : IClassFixture<DatabaseApiFactory>
{
    [Fact]
    public async Task Olustur_MesaiOneAlindiktanSonra_TamGunIzinliGuneSaatlikAcilamaz()
    {
        var admin = await TestAuth.OturumluClientAsync(factory, ApiFactory.SuperAdminEposta, ApiFactory.SuperAdminParola);
        var calisanId = await TestData.CalisanEkleAsync(admin, "Erkenci", await TestData.DepartmanEkleAsync(admin, "Mesai"));
        await TestData.HakEkleAsync(admin, calisanId, 14 * 480);
        var pazartesi = TestData.SonrakiYilPazartesi();
        var tamGun = await admin.PostAsJsonAsync(
            "/api/izin-talepleri",
            new { calisanId, izinTuruId = await TestData.IzinTuruIdAsync(admin, "Yıllık"), sureTipi = "TamGun", tarih = pazartesi },
            TestContext.Current.CancellationToken);

        var ayar = await admin.PutAsJsonAsync(
            "/api/ayarlar",
            new
            {
                calismaGunleri = new[] { "Pazartesi", "Sali", "Carsamba", "Persembe", "Cuma" },
                mesaiBaslangic = "08:00",
                mesaiBitis = "17:00",
                ogleArasiBaslangic = "12:00",
                ogleArasiBitis = "13:00",
                saatlikIzinAdimDakika = 30,
                hatirlatmaGunSayisi = 2
            },
            TestContext.Current.CancellationToken);
        var saatlik = await admin.PostAsJsonAsync(
            "/api/izin-talepleri",
            new
            {
                calisanId,
                izinTuruId = await TestData.IzinTuruIdAsync(admin, "Saatlik İzin"),
                sureTipi = "Saatlik",
                tarih = pazartesi,
                baslangicSaati = "08:00",
                bitisSaati = "09:00"
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, tamGun.StatusCode);
        Assert.Equal(HttpStatusCode.OK, ayar.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, saatlik.StatusCode);
        Assert.Equal(
            "Bu tarihlerle çakışan, beklemede ya da onaylanmış başka bir izin talebi var.",
            (await saatlik.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken)).GetProperty("detail").GetString());
    }
}
