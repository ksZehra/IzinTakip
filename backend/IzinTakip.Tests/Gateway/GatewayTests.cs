using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace IzinTakip.Tests.Gateway;

public sealed class GatewayTests(GatewayFactory factory) : IClassFixture<GatewayFactory>
{
    [Fact]
    public async Task Giris_GecerliBilgi_GuvenliCookieYazarVeApiIstegineTokenEkler()
    {
        var tarayici = factory.TarayiciClient();

        var giris = await GirisAsync(tarayici, ApiFactory.SuperAdminParola);
        var cerez = Assert.Single(giris.Headers.GetValues("Set-Cookie"));
        var ben = await tarayici.GetFromJsonAsync<JsonElement>("/api/auth/ben", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, giris.StatusCode);
        Assert.StartsWith("__Host-IzinTakip=", cerez);
        Assert.Contains("httponly", cerez, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("secure", cerez, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=strict", cerez, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("path=/", cerez, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("eyJ", cerez);
        Assert.Equal(ApiFactory.SuperAdminEposta, ben.GetProperty("eposta").GetString());
    }

    [Fact]
    public async Task Giris_HataliParola_ApininMesajiDonerCookieYazilmaz()
    {
        var giris = await GirisAsync(factory.TarayiciClient(), "YanlisParola1");

        Assert.Equal(HttpStatusCode.Unauthorized, giris.StatusCode);
        Assert.False(giris.Headers.Contains("Set-Cookie"));
        Assert.Equal("E-posta ya da parola hatalı. Üst üste hatalı denemede hesap birkaç dakika kilitlenir.", await DetayAsync(giris));
    }

    [Fact]
    public async Task CsrfBasligiYok_BffVeApiIstegi403Doner()
    {
        var tarayici = factory.TarayiciClient(csrfBasligi: false);

        var giris = await GirisAsync(tarayici, ApiFactory.SuperAdminParola);
        var api = await tarayici.GetAsync("/api/auth/ben", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, giris.StatusCode);
        Assert.Equal("İstek doğrulanamadı. Sayfayı yenileyip tekrar deneyin.", await DetayAsync(giris));
        Assert.Equal(HttpStatusCode.Forbidden, api.StatusCode);
    }

    [Fact]
    public async Task OturumYok_ApiIstegi401Doner()
    {
        var yanit = await factory.TarayiciClient().GetAsync("/api/auth/ben", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, yanit.StatusCode);
    }

    [Fact]
    public async Task Cikis_CookieSilinirVeSonrakiIstek401Doner()
    {
        var tarayici = factory.TarayiciClient();
        await GirisAsync(tarayici, ApiFactory.SuperAdminParola);

        var cikis = await tarayici.PostAsync("/bff/cikis", null, TestContext.Current.CancellationToken);
        var sonrasi = await tarayici.GetAsync("/api/auth/ben", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, cikis.StatusCode);
        Assert.Contains("expires=Thu, 01 Jan 1970", Assert.Single(cikis.Headers.GetValues("Set-Cookie")), StringComparison.OrdinalIgnoreCase);
        Assert.Equal(HttpStatusCode.Unauthorized, sonrasi.StatusCode);
    }

    [Fact]
    public async Task KurcalanmisCookie_401Doner()
    {
        var client = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost"), HandleCookies = false });
        client.DefaultRequestHeaders.Add("X-CSRF", "1");
        client.DefaultRequestHeaders.Add("Cookie", "__Host-IzinTakip=CfDJ8bozukdeger");

        var yanit = await client.GetAsync("/api/auth/ben", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, yanit.StatusCode);
    }

    [Fact]
    public async Task TarayicininAuthorizationBasligi_ApiyeIletilmez()
    {
        var token = await TestKimlik.TokenAlAsync(factory.Api.CreateClient(), ApiFactory.SuperAdminEposta, ApiFactory.SuperAdminParola);
        var tarayici = factory.TarayiciClient();
        tarayici.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var yanit = await tarayici.GetAsync("/api/auth/ben", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, yanit.StatusCode);
    }

    [Fact]
    public async Task GuvenlikBasliklari_HerYanittaVarHealthCsrfIstemez()
    {
        var yanit = await factory.TarayiciClient(csrfBasligi: false).GetAsync("/health", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, yanit.StatusCode);
        Assert.Equal("nosniff", yanit.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("DENY", yanit.Headers.GetValues("X-Frame-Options").Single());
        Assert.Contains("frame-ancestors 'none'", yanit.Headers.GetValues("Content-Security-Policy").Single());
    }

    [Theory]
    [InlineData("/api/auth/login")]
    [InlineData("/API/Auth/Login/")]
    public async Task ApiGirisUcu_GatewayUzerindenKapali_404Doner(string yol)
    {
        var yanit = await factory.TarayiciClient().PostAsJsonAsync(
            yol,
            new { eposta = ApiFactory.SuperAdminEposta, parola = ApiFactory.SuperAdminParola },
            TestContext.Current.CancellationToken);
        var govde = await yanit.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, yanit.StatusCode);
        Assert.DoesNotContain("accessToken", govde);
    }

    private static Task<HttpResponseMessage> GirisAsync(HttpClient client, string parola) =>
        client.PostAsJsonAsync("/bff/giris", new { eposta = ApiFactory.SuperAdminEposta, parola }, TestContext.Current.CancellationToken);

    private static async Task<string?> DetayAsync(HttpResponseMessage yanit) =>
        (await yanit.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken)).GetProperty("detail").GetString();
}
