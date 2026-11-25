using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace IzinTakip.Tests.Features.IzinTalepleri;

public sealed class IzinTalebiListeTests(DatabaseApiFactory factory) : IClassFixture<DatabaseApiFactory>
{
    private const int GunlukDakika = 480;
    private static readonly DateOnly Pazartesi = TestData.SonrakiYilPazartesi();

    [Fact]
    public async Task Listele_CalisanAdiCokluDurumVeTur_YalnizEslesenleriGetirir()
    {
        var (kod, admin, turler) = await TalepleriKurAsync();

        var ayse = await ListeAsync(admin, $"calisanArama=Ayse{kod}");
        var iptal = await ListeAsync(admin, $"calisanArama={kod}&durum=IptalEdildi");
        var acik = await ListeAsync(admin, $"calisanArama={kod}&durum=Beklemede&durum=IptalEdildi");
        var ucretsiz = await ListeAsync(admin, $"calisanArama={kod}&izinTuruId={turler.Ucretsiz}");
        var ikiTur = await ListeAsync(admin, $"calisanArama={kod}&izinTuruId={turler.Yillik}&izinTuruId={turler.Ucretsiz}");

        Assert.Equal(2, ayse.GetProperty("totalCount").GetInt32());
        Assert.Equal(["Ücretsiz"], Turler(iptal));
        Assert.Equal(3, acik.GetProperty("totalCount").GetInt32());
        Assert.Equal(["Ücretsiz"], Turler(ucretsiz));
        Assert.Equal(3, ikiTur.GetProperty("totalCount").GetInt32());
    }

    [Fact]
    public async Task Listele_TarihAraligi_IzinGunuKesisenleriGetirir()
    {
        var (kod, admin, _) = await TalepleriKurAsync();
        var gun = Pazartesi.AddDays(7).ToString("yyyy-MM-dd");

        var ayniGun = await ListeAsync(admin, $"calisanArama={kod}&baslangic={gun}&bitis={gun}");
        var sonrasi = await ListeAsync(admin, $"calisanArama={kod}&baslangic={gun}");

        Assert.Equal(["Ücretsiz"], Turler(ayniGun));
        Assert.Equal(2, sonrasi.GetProperty("totalCount").GetInt32());
    }

    [Fact]
    public async Task Listele_Sirala_SecilenAlanaGoreDonerVarsayilanYenidenEskiye()
    {
        var (kod, admin, _) = await TalepleriKurAsync();

        var tarihe = await ListeAsync(admin, $"calisanArama={kod}&sort=baslangic:asc");
        var ada = await ListeAsync(admin, $"calisanArama={kod}&sort=calisanAdSoyad:desc");
        var duruma = await ListeAsync(admin, $"calisanArama={kod}&sort=durum:desc");
        var varsayilan = await ListeAsync(admin, $"calisanArama={kod}");

        Assert.Equal(["Yıllık", "Ücretsiz", "Yıllık"], Turler(tarihe));
        Assert.StartsWith("Burak", Calisanlar(ada)[0]);
        Assert.Equal("IptalEdildi", duruma.GetProperty("items")[0].GetProperty("durum").GetString());
        Assert.Equal(Idler(tarihe).Reverse(), Idler(varsayilan));
    }

    [Fact]
    public async Task Listele_DurumaGoreSirala_EtiketinAlfabetikSirasiniIzler()
    {
        var (kod, admin, turler) = await TalepleriKurAsync();
        var burak = await TestData.CalisanEkleAsync(admin, $"Burak2{kod}", await TestData.DepartmanEkleAsync(admin, $"D2-{kod}"));
        await TestData.HakEkleAsync(admin, burak, 14 * GunlukDakika);
        var onaylanacak = await TalepAcAsync(admin, burak, turler.Yillik, Pazartesi.AddDays(21));
        var onay = await admin.PostAsJsonAsync($"/api/izin-talepleri/{onaylanacak}/onayla", new { }, TestContext.Current.CancellationToken);
        onay.EnsureSuccessStatusCode();

        var artan = await ListeAsync(admin, $"calisanArama={kod}&sort=durum:asc");

        Assert.Equal(
            ["Beklemede", "Beklemede", "IptalEdildi", "Onaylandi"],
            artan.GetProperty("items").EnumerateArray().Select(t => t.GetProperty("durum").GetString()));
    }

    [Theory]
    [InlineData("sort=aciklama:asc", "sort")]
    [InlineData("sort=baslangic", "sort")]
    [InlineData("baslangic=2030-02-01&bitis=2030-01-01", "bitis")]
    public async Task Listele_GecersizSiralamaVeAralik_400Doner(string sorgu, string alan)
    {
        var admin = await SuperAdminAsync();

        var yanit = await admin.GetAsync($"/api/izin-talepleri?{sorgu}", TestContext.Current.CancellationToken);
        var govde = await yanit.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, yanit.StatusCode);
        Assert.True(govde.GetProperty("errors").TryGetProperty(alan, out _));
    }

    [Fact]
    public async Task Listele_CokFazlaSecim_400Doner()
    {
        var admin = await SuperAdminAsync();
        var sorgu = string.Join("&", Enumerable.Range(1, 101).Select(id => $"izinTuruId={id}"));

        var yanit = await admin.GetAsync($"/api/izin-talepleri?{sorgu}", TestContext.Current.CancellationToken);
        var govde = await yanit.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, yanit.StatusCode);
        Assert.True(govde.GetProperty("errors").TryGetProperty("izinTuruId", out _));
    }

    [Fact]
    public async Task Excel_Siralama_ListeIleAyniSiradaYazar()
    {
        var (kod, admin, _) = await TalepleriKurAsync();

        var yanit = await admin.GetAsync($"/api/izin-talepleri/excel?calisanArama={kod}&sort=baslangic:asc", TestContext.Current.CancellationToken);
        using var kitap = new ClosedXML.Excel.XLWorkbook(await yanit.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken));
        var sayfa = kitap.Worksheet(1);

        Assert.Equal(HttpStatusCode.OK, yanit.StatusCode);
        Assert.Equal(["Yıllık", "Ücretsiz", "Yıllık"], Enumerable.Range(2, 3).Select(satir => sayfa.Cell(satir, 3).GetString()));
    }

    // Ayşe: yıllık (Pazartesi, beklemede) ve ücretsiz (+7 gün, iptal); Burak: yıllık (+14 gün, beklemede).
    private async Task<(string Kod, HttpClient Admin, (int Yillik, int Ucretsiz) Turler)> TalepleriKurAsync()
    {
        var kod = Guid.NewGuid().ToString("N")[..8];
        var admin = await SuperAdminAsync();
        var departmanId = await TestData.DepartmanEkleAsync(admin, $"Departman-{kod}");
        var ayse = await TestData.CalisanEkleAsync(admin, $"Ayse{kod}", departmanId);
        var burak = await TestData.CalisanEkleAsync(admin, $"Burak{kod}", departmanId);
        await TestData.HakEkleAsync(admin, ayse, 14 * GunlukDakika);
        await TestData.HakEkleAsync(admin, burak, 14 * GunlukDakika);
        var yillik = await TestData.IzinTuruIdAsync(admin, "Yıllık");
        var ucretsiz = await TestData.IzinTuruIdAsync(admin, "Ücretsiz");

        await TalepAcAsync(admin, ayse, yillik, Pazartesi);
        var iptalEdilecek = await TalepAcAsync(admin, ayse, ucretsiz, Pazartesi.AddDays(7));
        await TalepAcAsync(admin, burak, yillik, Pazartesi.AddDays(14));
        var iptal = await admin.PostAsJsonAsync($"/api/izin-talepleri/{iptalEdilecek}/iptal", new { }, TestContext.Current.CancellationToken);
        iptal.EnsureSuccessStatusCode();
        return (kod, admin, (yillik, ucretsiz));
    }

    private static async Task<int> TalepAcAsync(HttpClient client, int calisanId, int izinTuruId, DateOnly tarih)
    {
        var yanit = await client.PostAsJsonAsync(
            "/api/izin-talepleri",
            new { calisanId, izinTuruId, sureTipi = "TamGun", tarih },
            TestContext.Current.CancellationToken);
        var govde = await yanit.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.True(yanit.StatusCode == HttpStatusCode.Created, govde);
        return JsonDocument.Parse(govde).RootElement.GetProperty("id").GetInt32();
    }

    private static async Task<JsonElement> ListeAsync(HttpClient istemci, string sorgu) =>
        await istemci.GetFromJsonAsync<JsonElement>($"/api/izin-talepleri?{sorgu}", TestContext.Current.CancellationToken);

    private static string[] Turler(JsonElement liste) =>
        liste.GetProperty("items").EnumerateArray().Select(t => t.GetProperty("izinTuruAdi").GetString()!).ToArray();

    private static int[] Idler(JsonElement liste) =>
        liste.GetProperty("items").EnumerateArray().Select(t => t.GetProperty("id").GetInt32()).ToArray();

    private static string[] Calisanlar(JsonElement liste) =>
        liste.GetProperty("items").EnumerateArray().Select(t => t.GetProperty("calisanAdSoyad").GetString()!).ToArray();

    private Task<HttpClient> SuperAdminAsync() =>
        TestAuth.OturumluClientAsync(factory, ApiFactory.SuperAdminEposta, ApiFactory.SuperAdminParola);
}
