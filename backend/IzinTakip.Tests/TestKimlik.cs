using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using IzinTakip.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace IzinTakip.Tests;

public static class TestKimlik
{
    public const string VarsayilanParola = "Deneme2026";

    public static async Task<string> TokenAlAsync(HttpClient client, string eposta, string parola)
    {
        var yanit = await client.PostAsJsonAsync("/api/auth/login", new { eposta, parola }, TestContext.Current.CancellationToken);
        yanit.EnsureSuccessStatusCode();
        var govde = await yanit.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        return govde.GetProperty("accessToken").GetString()!;
    }

    public static async Task<HttpClient> OturumluClientAsync(WebApplicationFactory<Program> factory, string eposta, string parola = VarsayilanParola)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await TokenAlAsync(client, eposta, parola));
        return client;
    }

    public static async Task<int> KullaniciOlusturAsync(
        WebApplicationFactory<Program> factory,
        string eposta,
        string rol,
        bool parolaDegistirmeli = false,
        int? calisanId = null)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Kullanici>>();
        var kullanici = new Kullanici
        {
            UserName = eposta,
            Email = eposta,
            ParolaDegistirmeliMi = parolaDegistirmeli,
            CalisanId = calisanId
        };

        var sonuc = await userManager.CreateAsync(kullanici, VarsayilanParola);
        Assert.True(sonuc.Succeeded, string.Join(" ", sonuc.Errors.Select(e => e.Description)));
        Assert.True((await userManager.AddToRoleAsync(kullanici, rol)).Succeeded);
        return kullanici.Id;
    }
}
