using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using IzinTakip.Domain;

namespace IzinTakip.Tests.Features.Guvenlik;

public sealed class KapsamDisiErisimTests(DatabaseApiFactory factory) : IClassFixture<DatabaseApiFactory>
{
    private const int GunlukDakika = 480;
    private static readonly DateOnly Pazartesi = TestData.SonrakiYilPazartesi();

    [Fact]
    public async Task Yonetici_BaskaDepartmaninKayitlarinaOkumaVeYazmaIstegi_KayitVermez()
    {
        var (yonetici, ekipCalisani, disaridaki, disaridakiTalep) = await KurAsync();
        var ct = TestContext.Current.CancellationToken;

        var istekler = new (string Ad, Func<Task<HttpResponseMessage>> Gonder)[]
        {
            ("çalışan detayı", () => yonetici.GetAsync($"/api/calisanlar/{disaridaki}", ct)),
            ("hak satırları", () => yonetici.GetAsync($"/api/calisanlar/{disaridaki}/haklar", ct)),
            ("bakiye", () => yonetici.GetAsync($"/api/calisanlar/{disaridaki}/bakiye", ct)),
            ("talep detayı", () => yonetici.GetAsync($"/api/izin-talepleri/{disaridakiTalep}", ct)),
            ("talep belgeleri", () => yonetici.GetAsync($"/api/izin-talepleri/{disaridakiTalep}/belgeler", ct)),
            ("talep onayı", () => yonetici.PostAsJsonAsync($"/api/izin-talepleri/{disaridakiTalep}/onayla", new { }, ct)),
            ("talep reddi", () => yonetici.PostAsJsonAsync($"/api/izin-talepleri/{disaridakiTalep}/reddet", new { gerekce = "Uygun değil" }, ct)),
            ("talep iptali", () => yonetici.PostAsJsonAsync($"/api/izin-talepleri/{disaridakiTalep}/iptal", new { }, ct))
        };

        foreach (var (ad, gonder) in istekler)
        {
            var yanit = await gonder();
            Assert.True(
                yanit.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Forbidden,
                $"{ad}: {(int)yanit.StatusCode} döndü, 404 ya da 403 bekleniyordu.");
        }

        var kendiEkibi = await yonetici.GetAsync($"/api/calisanlar/{ekipCalisani}", ct);
        Assert.Equal(HttpStatusCode.OK, kendiEkibi.StatusCode);
    }

    [Fact]
    public async Task Yonetici_VarOlmayanVeYetkisizKayit_AyniCevabiAlir()
    {
        var (yonetici, _, disaridaki, _) = await KurAsync();
        var ct = TestContext.Current.CancellationToken;

        var yetkisiz = await yonetici.GetAsync($"/api/calisanlar/{disaridaki}", ct);
        var olmayan = await yonetici.GetAsync($"/api/calisanlar/{int.MaxValue}", ct);

        Assert.Equal(olmayan.StatusCode, yetkisiz.StatusCode);
        Assert.Equal(
            (await olmayan.Content.ReadFromJsonAsync<JsonElement>(ct)).GetProperty("detail").GetString(),
            (await yetkisiz.Content.ReadFromJsonAsync<JsonElement>(ct)).GetProperty("detail").GetString());
    }

    private async Task<(HttpClient Yonetici, int EkipCalisani, int Disaridaki, int DisaridakiTalep)> KurAsync()
    {
        var kod = Guid.NewGuid().ToString("N")[..8];
        var admin = await TestAuth.OturumluClientAsync(factory, ApiFactory.SuperAdminEposta, ApiFactory.SuperAdminParola);
        var ekip = await TestData.DepartmanEkleAsync(admin, $"Ekip-{kod}");
        var diger = await TestData.DepartmanEkleAsync(admin, $"Diger-{kod}");
        var yoneticiId = await TestData.CalisanEkleAsync(admin, $"Yonetici{kod}", ekip);
        await TestData.DepartmanYoneticisiAtaAsync(admin, ekip, $"Ekip-{kod}", yoneticiId);
        var ekipCalisani = await TestData.CalisanEkleAsync(admin, $"Ekip{kod}", ekip);
        var disaridaki = await TestData.CalisanEkleAsync(admin, $"Disarida{kod}", diger);
        await TestData.HakEkleAsync(admin, disaridaki, 14 * GunlukDakika);

        var yillik = await TestData.IzinTuruIdAsync(admin, "Yıllık");
        var talep = await admin.PostAsJsonAsync(
            "/api/izin-talepleri",
            new { calisanId = disaridaki, izinTuruId = yillik, sureTipi = "TamGun", tarih = Pazartesi },
            TestContext.Current.CancellationToken);
        talep.EnsureSuccessStatusCode();
        var talepId = (await talep.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken)).GetProperty("id").GetInt32();

        var eposta = $"yonetici-{kod}@izintakip.test";
        await TestAuth.KullaniciOlusturAsync(factory, eposta, SistemRolleri.Yonetici, calisanId: yoneticiId);
        return (await TestAuth.OturumluClientAsync(factory, eposta), ekipCalisani, disaridaki, talepId);
    }
}
