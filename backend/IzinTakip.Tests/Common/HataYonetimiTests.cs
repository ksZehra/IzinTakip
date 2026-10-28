using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace IzinTakip.Tests.Common;

public sealed class HataYonetimiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task IsKuraliHatasi_422VeKullaniciMesajiDoner()
    {
        var yanit = await _client.GetAsync("/test/hata/is-kurali", TestContext.Current.CancellationToken);
        var problem = await ProblemOku(yanit);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, yanit.StatusCode);
        Assert.Equal("Bakiyeniz 3 gün, talep 5 gün.", problem.GetProperty("detail").GetString());
        Assert.Matches("^[0-9a-f]{32}$", problem.GetProperty("traceId").GetString());
    }

    [Fact]
    public async Task KayitBulunamadiHatasi_404VeKullaniciMesajiDoner()
    {
        var yanit = await _client.GetAsync("/test/hata/bulunamadi", TestContext.Current.CancellationToken);
        var problem = await ProblemOku(yanit);

        Assert.Equal(HttpStatusCode.NotFound, yanit.StatusCode);
        Assert.Equal("İzin talebi bulunamadı.", problem.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task BeklenmeyenHata_500VeTeknikAyrintiSizdirmaz()
    {
        var yanit = await _client.GetAsync("/test/hata/beklenmeyen", TestContext.Current.CancellationToken);
        var govde = await yanit.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.InternalServerError, yanit.StatusCode);
        Assert.DoesNotContain("Gizli teknik", govde);
        Assert.DoesNotContain("InvalidOperationException", govde);

        using var problem = JsonDocument.Parse(govde);
        var traceId = problem.RootElement.GetProperty("traceId").GetString()!;
        Assert.Equal(
            $"Beklenmeyen bir hata oluştu. Destek için bu kodu iletin: {traceId}",
            problem.RootElement.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task DogrulamaHatasi_400VeAlanHatasiDoner()
    {
        var yanit = await _client.PostAsJsonAsync("/test/hata/dogrulama", new { ad = "" }, TestContext.Current.CancellationToken);
        var problem = await ProblemOku(yanit);

        Assert.Equal(HttpStatusCode.BadRequest, yanit.StatusCode);
        Assert.Equal("Ad zorunludur.", problem.GetProperty("errors").GetProperty("ad")[0].GetString());
    }

    [Fact]
    public async Task BilinmeyenAdres_OturumsuzIstekte401AyniBicimdeDoner()
    {
        var yanit = await _client.GetAsync("/boyle-bir-adres-yok", TestContext.Current.CancellationToken);
        var problem = await ProblemOku(yanit);

        Assert.Equal(HttpStatusCode.Unauthorized, yanit.StatusCode);
        Assert.Equal(401, problem.GetProperty("status").GetInt32());
    }

    private static async Task<JsonElement> ProblemOku(HttpResponseMessage yanit)
    {
        Assert.Equal("application/problem+json", yanit.Content.Headers.ContentType?.MediaType);
        return await yanit.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
    }
}
