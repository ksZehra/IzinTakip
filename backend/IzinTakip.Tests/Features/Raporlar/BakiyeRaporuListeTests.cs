using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ClosedXML.Excel;

namespace IzinTakip.Tests.Features.Raporlar;

public sealed class BakiyeRaporuListeTests(DatabaseApiFactory factory) : IClassFixture<DatabaseApiFactory>
{
    [Fact]
    public async Task Bakiyeler_AdCokluDepartmanVeIseGiris_YalnizEslesenleriGetirir()
    {
        var (kod, admin, ilk, ikinci) = await CalisanlariKurAsync();

        var ada = await ListeAsync(admin, $"departmanId={ilk}&departmanId={ikinci}&arama=Mert{kod}");
        var ikiDepartman = await ListeAsync(admin, $"departmanId={ilk}&departmanId={ikinci}");
        var tekDepartman = await ListeAsync(admin, $"departmanId={ikinci}");
        var araliktakiler = await ListeAsync(admin, $"departmanId={ilk}&departmanId={ikinci}&iseGirisBaslangic=2019-01-01&iseGirisBitis=2021-12-31");

        Assert.Equal([$"Mert{kod} Test"], Adlar(ada));
        Assert.Equal(3, ikiDepartman.GetProperty("totalCount").GetInt32());
        Assert.Equal([$"Zeynep{kod} Test"], Adlar(tekDepartman));
        Assert.Equal([$"Mert{kod} Test"], Adlar(araliktakiler));
    }

    [Fact]
    public async Task Bakiyeler_Sirala_SecilenAlanaGoreDoner()
    {
        var (kod, admin, ilk, ikinci) = await CalisanlariKurAsync();
        var departmanlar = $"departmanId={ilk}&departmanId={ikinci}";

        var varsayilan = await ListeAsync(admin, departmanlar);
        var adaAzalan = await ListeAsync(admin, $"{departmanlar}&sort=calisanAdSoyad:desc");
        var tarihe = await ListeAsync(admin, $"{departmanlar}&sort=iseGirisTarihi:asc");

        Assert.Equal([$"Ahmet{kod} Test", $"Mert{kod} Test", $"Zeynep{kod} Test"], Adlar(varsayilan));
        Assert.Equal([$"Zeynep{kod} Test", $"Mert{kod} Test", $"Ahmet{kod} Test"], Adlar(adaAzalan));
        Assert.Equal([$"Zeynep{kod} Test", $"Mert{kod} Test", $"Ahmet{kod} Test"], Adlar(tarihe));
    }

    [Theory]
    [InlineData("sort=kullanilabilirDakika:asc", "sort")]
    [InlineData("iseGirisBaslangic=2022-01-01&iseGirisBitis=2021-01-01", "iseGirisBitis")]
    public async Task Bakiyeler_GecersizSiralamaVeAralik_400Doner(string sorgu, string alan)
    {
        var admin = await SuperAdminAsync();

        var yanit = await admin.GetAsync($"/api/raporlar/bakiyeler?{sorgu}", TestContext.Current.CancellationToken);
        var govde = await yanit.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, yanit.StatusCode);
        Assert.True(govde.GetProperty("errors").TryGetProperty(alan, out _));
    }

    [Fact]
    public async Task BakiyeExcel_FiltreVeSiralama_ListeIleAyniSatirlariYazar()
    {
        var (kod, admin, ilk, ikinci) = await CalisanlariKurAsync();

        var yanit = await admin.GetAsync(
            $"/api/raporlar/bakiyeler/excel?departmanId={ilk}&departmanId={ikinci}&sort=calisanAdSoyad:desc&iseGirisBaslangic=2019-01-01",
            TestContext.Current.CancellationToken);
        using var kitap = new XLWorkbook(await yanit.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken));
        var sayfa = kitap.Worksheet(1);

        Assert.Equal(HttpStatusCode.OK, yanit.StatusCode);
        Assert.Equal(3, sayfa.LastRowUsed()!.RowNumber());
        Assert.Equal($"Mert{kod} Test", sayfa.Cell(2, 1).GetString());
        Assert.Equal($"Ahmet{kod} Test", sayfa.Cell(3, 1).GetString());
    }

    // İlk departman: Ahmet (2022) ve Mert (2020); ikinci departman: Zeynep (2018).
    private async Task<(string Kod, HttpClient Admin, int Ilk, int Ikinci)> CalisanlariKurAsync()
    {
        var kod = Guid.NewGuid().ToString("N")[..8];
        var admin = await SuperAdminAsync();
        var ilk = await TestData.DepartmanEkleAsync(admin, $"Rapor-{kod}-1");
        var ikinci = await TestData.DepartmanEkleAsync(admin, $"Rapor-{kod}-2");
        await TestData.CalisanEkleAsync(admin, $"Ahmet{kod}", ilk, new DateOnly(2022, 1, 3));
        await TestData.CalisanEkleAsync(admin, $"Mert{kod}", ilk, new DateOnly(2020, 6, 1));
        await TestData.CalisanEkleAsync(admin, $"Zeynep{kod}", ikinci, new DateOnly(2018, 3, 5));
        return (kod, admin, ilk, ikinci);
    }

    private static async Task<JsonElement> ListeAsync(HttpClient istemci, string sorgu) =>
        await istemci.GetFromJsonAsync<JsonElement>($"/api/raporlar/bakiyeler?{sorgu}", TestContext.Current.CancellationToken);

    private static string[] Adlar(JsonElement liste) =>
        liste.GetProperty("items").EnumerateArray().Select(s => s.GetProperty("calisanAdSoyad").GetString()!).ToArray();

    private Task<HttpClient> SuperAdminAsync() =>
        TestAuth.OturumluClientAsync(factory, ApiFactory.SuperAdminEposta, ApiFactory.SuperAdminParola);
}
