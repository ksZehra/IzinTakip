using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace IzinTakip.Tests.Gateway;

public sealed class GirisHizSiniriTests(GirisHizSiniriTests.Factory factory) : IClassFixture<GirisHizSiniriTests.Factory>
{
    [Fact]
    public async Task Giris_DakikadaBesDenemedenSonra_429Doner()
    {
        var tarayici = factory.TarayiciClient();
        var durumlar = new List<HttpStatusCode>();
        HttpResponseMessage? son = null;

        for (var deneme = 0; deneme < 6; deneme++)
        {
            son = await tarayici.PostAsJsonAsync(
                "/bff/giris",
                new { eposta = "olmayan@izintakip.test", parola = "Parola2026" },
                TestContext.Current.CancellationToken);
            durumlar.Add(son.StatusCode);
        }

        Assert.All(durumlar.Take(5), d => Assert.Equal(HttpStatusCode.Unauthorized, d));
        Assert.Equal(HttpStatusCode.TooManyRequests, durumlar[5]);
        Assert.Equal(
            "Çok fazla giriş denemesi yapıldı. Bir dakika sonra tekrar deneyin.",
            (await son!.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken)).GetProperty("detail").GetString());
    }

    public sealed class Factory : GatewayFactory
    {
        protected override int DakikadaGirisDenemesi => 5;
    }
}
