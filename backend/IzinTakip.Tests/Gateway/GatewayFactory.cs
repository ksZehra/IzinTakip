extern alias GatewayApp;

using System.Globalization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Yarp.ReverseProxy.Forwarder;

namespace IzinTakip.Tests.Gateway;

public class GatewayFactory : WebApplicationFactory<GatewayApp::Program>, IAsyncLifetime
{
    public DatabaseApiFactory Api { get; } = new();

    protected virtual int DakikadaGirisDenemesi => 1000;

    public ValueTask InitializeAsync() => Api.InitializeAsync();

    public HttpClient TarayiciClient(bool csrfBasligi = true)
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), HandleCookies = true });
        if (csrfBasligi)
        {
            client.DefaultRequestHeaders.Add("X-CSRF", "1");
        }

        return client;
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await Api.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    public static void TemelAyarlar(IWebHostBuilder builder, string apiAdresi)
    {
        builder.UseEnvironment("Test");
        builder.UseSetting("Api:Adres", apiAdresi);
        builder.UseSetting("DataProtection:Klasor", Path.Combine(Path.GetTempPath(), "IzinTakip.Tests", "gateway-keys"));
        builder.UseSetting("Serilog:MinimumLevel:Default", "Warning");
        builder.UseSetting("LogKlasoru", Path.Combine(Path.GetTempPath(), "IzinTakip.Tests", "logs"));
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        TemelAyarlar(builder, "http://api.test");
        builder.UseSetting("GirisSiniri:DakikadaDeneme", DakikadaGirisDenemesi.ToString(CultureInfo.InvariantCulture));

        builder.ConfigureServices(services =>
        {
            services.AddHttpClient("api").ConfigurePrimaryHttpMessageHandler(() => Api.Server.CreateHandler());
            services.AddSingleton<IForwarderHttpClientFactory>(new ApiForwarderHttpClientFactory(Api));
        });
    }

    private sealed class ApiForwarderHttpClientFactory(DatabaseApiFactory api) : IForwarderHttpClientFactory
    {
        public HttpMessageInvoker CreateClient(ForwarderHttpClientContext context) => new(api.Server.CreateHandler());
    }
}
