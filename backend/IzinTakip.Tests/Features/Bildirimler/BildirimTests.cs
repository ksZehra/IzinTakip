using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using IzinTakip.Domain;
using IzinTakip.Domain.Entities;
using IzinTakip.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace IzinTakip.Tests.Features.Bildirimler;

public sealed class BildirimTests(DatabaseApiFactory factory) : IClassFixture<DatabaseApiFactory>
{
    [Fact]
    public async Task Listele_YalnizKendiBildirimleriniYenidenEskiyeDoner()
    {
        var (kullaniciId, client) = await KullaniciKurAsync();
        var (digerId, _) = await KullaniciKurAsync();
        await BildirimEkleAsync(kullaniciId, "Birinci", "İkinci");
        await BildirimEkleAsync(digerId, "Başkasının");

        var liste = await client.GetFromJsonAsync<JsonElement>("/api/bildirimler", TestContext.Current.CancellationToken);

        Assert.Equal(2, liste.GetProperty("totalCount").GetInt32());
        Assert.Equal("İkinci", liste.GetProperty("items")[0].GetProperty("baslik").GetString());
        Assert.False(liste.GetProperty("items")[0].GetProperty("okunduMu").GetBoolean());
    }

    [Fact]
    public async Task OkunduIsaretle_TekVeTumu_SayiyiAzaltir()
    {
        var (kullaniciId, client) = await KullaniciKurAsync();
        var idler = await BildirimEkleAsync(kullaniciId, "Bir", "İki", "Üç");

        var tek = await client.PutAsync($"/api/bildirimler/{idler[0]}/okundu", null, TestContext.Current.CancellationToken);
        var sayiTektenSonra = await OkunmamisAsync(client);
        var okunmamislar = await client.GetFromJsonAsync<JsonElement>("/api/bildirimler?sadeceOkunmamis=true", TestContext.Current.CancellationToken);
        var tumu = await client.PutAsync("/api/bildirimler/okundu", null, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, tek.StatusCode);
        Assert.Equal(2, sayiTektenSonra);
        Assert.Equal(2, okunmamislar.GetProperty("totalCount").GetInt32());
        Assert.Equal(HttpStatusCode.NoContent, tumu.StatusCode);
        Assert.Equal(0, await OkunmamisAsync(client));
    }

    [Fact]
    public async Task OkunduIsaretle_BaskasininBildirimi_404Doner()
    {
        var (_, client) = await KullaniciKurAsync();
        var (digerId, _) = await KullaniciKurAsync();
        var idler = await BildirimEkleAsync(digerId, "Başkasının");

        var yanit = await client.PutAsync($"/api/bildirimler/{idler[0]}/okundu", null, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, yanit.StatusCode);
    }

    private async Task<(int KullaniciId, HttpClient Client)> KullaniciKurAsync()
    {
        var eposta = $"kullanici-{Guid.NewGuid():N}@izintakip.test";
        var kullaniciId = await TestAuth.KullaniciOlusturAsync(factory, eposta, SistemRolleri.Calisan);
        return (kullaniciId, await TestAuth.OturumluClientAsync(factory, eposta));
    }

    private async Task<int[]> BildirimEkleAsync(int kullaniciId, params string[] basliklar)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IzinTakipDbContext>();
        var bildirimler = basliklar.Select(b => new Bildirim(kullaniciId, b, $"{b} metni", null)).ToList();
        foreach (var bildirim in bildirimler)
        {
            db.Bildirimler.Add(bildirim);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        return bildirimler.Select(b => b.Id).ToArray();
    }

    private static async Task<int> OkunmamisAsync(HttpClient client) =>
        (await client.GetFromJsonAsync<JsonElement>("/api/bildirimler/sayi", TestContext.Current.CancellationToken)).GetProperty("okunmamis").GetInt32();
}
