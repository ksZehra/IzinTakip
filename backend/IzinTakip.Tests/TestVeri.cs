using System.Net.Http.Json;
using System.Text.Json;

namespace IzinTakip.Tests;

public static class TestVeri
{
    public static async Task<int> DepartmanEkleAsync(HttpClient yetkili, string ad, int? yoneticiId = null)
    {
        var yanit = await yetkili.PostAsJsonAsync("/api/departmanlar", new { ad, yoneticiId }, TestContext.Current.CancellationToken);
        return await IdOkuAsync(yanit);
    }

    public static async Task DepartmanYoneticisiAtaAsync(HttpClient yetkili, int departmanId, string ad, int yoneticiId)
    {
        var yanit = await yetkili.PutAsJsonAsync($"/api/departmanlar/{departmanId}", new { ad, yoneticiId }, TestContext.Current.CancellationToken);
        yanit.EnsureSuccessStatusCode();
    }

    public static async Task<int> CalisanEkleAsync(
        HttpClient yetkili,
        string ad,
        int departmanId,
        DateOnly? iseGirisTarihi = null,
        DateOnly? dogumTarihi = null)
    {
        var yanit = await yetkili.PostAsJsonAsync(
            "/api/calisanlar",
            new
            {
                ad,
                soyad = "Test",
                departmanId,
                iseGirisTarihi = iseGirisTarihi ?? new DateOnly(2020, 1, 6),
                dogumTarihi = dogumTarihi ?? new DateOnly(1990, 5, 15)
            },
            TestContext.Current.CancellationToken);
        return await IdOkuAsync(yanit);
    }

    private static async Task<int> IdOkuAsync(HttpResponseMessage yanit)
    {
        var govde = await yanit.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.True(yanit.IsSuccessStatusCode, govde);
        return JsonDocument.Parse(govde).RootElement.GetProperty("id").GetInt32();
    }
}
