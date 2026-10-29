extern alias GatewayApp;

using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text.Json;
using GatewayApp::IzinTakip.Gateway.Bff;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace IzinTakip.Tests.Gateway;

public sealed class ApiKapaliTests(ApiKapaliTests.Factory factory) : IClassFixture<ApiKapaliTests.Factory>
{
    private const string UlasilamiyorMesaji = "Sunucuya şu anda ulaşılamıyor. Biraz sonra tekrar deneyin.";

    [Fact]
    public async Task Giris_ApiKapaliyken_502VeTurkceMesajDoner()
    {
        var yanit = await factory.Tarayici().PostAsJsonAsync(
            "/bff/giris",
            new { eposta = "kisi@izintakip.test", parola = "Parola2026" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadGateway, yanit.StatusCode);
        Assert.Equal(UlasilamiyorMesaji, await DetayAsync(yanit));
    }

    [Fact]
    public async Task ApiIstegi_ApiKapaliyken_502VeTurkceMesajDoner()
    {
        var yanit = await factory.Tarayici().GetAsync("/api/auth/ben", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadGateway, yanit.StatusCode);
        Assert.Equal("application/problem+json", yanit.Content.Headers.ContentType?.MediaType);
        Assert.Equal(UlasilamiyorMesaji, await DetayAsync(yanit));
    }

    [Fact]
    public async Task Cikis_ApiKapaliyken_CookieYineDeSilinir()
    {
        var http = new DefaultHttpContext();
        factory.Services.GetRequiredService<OturumCerezi>().Yaz(http.Response, "gecerli-token", DateTimeOffset.UtcNow.AddHours(1));
        var tarayici = factory.Tarayici();
        tarayici.DefaultRequestHeaders.Add("Cookie", http.Response.Headers.SetCookie.ToString().Split(';')[0]);

        var yanit = await tarayici.PostAsync("/bff/cikis", null, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, yanit.StatusCode);
        Assert.Contains("expires=Thu, 01 Jan 1970", Assert.Single(yanit.Headers.GetValues("Set-Cookie")), StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<string?> DetayAsync(HttpResponseMessage yanit) =>
        (await yanit.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken)).GetProperty("detail").GetString();

    public sealed class Factory : WebApplicationFactory<GatewayApp::Program>
    {
        public HttpClient Tarayici()
        {
            var client = CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
            client.DefaultRequestHeaders.Add("X-CSRF", "1");
            return client;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder) =>
            GatewayFactory.TemelAyarlar(builder, $"http://127.0.0.1:{KapaliPort()}");

        private static int KapaliPort()
        {
            using var dinleyici = new TcpListener(IPAddress.Loopback, 0);
            dinleyici.Start();
            return ((IPEndPoint)dinleyici.LocalEndpoint).Port;
        }
    }
}
