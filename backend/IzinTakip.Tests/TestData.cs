using System.Net.Http.Json;
using System.Text.Json;

namespace IzinTakip.Tests;

public static class TestData
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

    public static async Task HakEkleAsync(HttpClient yetkili, int calisanId, int dakika)
    {
        var yanit = await yetkili.PostAsJsonAsync(
            $"/api/calisanlar/{calisanId}/haklar",
            new { dakika, aciklama = "Açılış bakiyesi" },
            TestContext.Current.CancellationToken);
        yanit.EnsureSuccessStatusCode();
    }

    public static async Task<int> IzinTuruIdAsync(HttpClient client, string ad)
    {
        var turler = await client.GetFromJsonAsync<JsonElement>("/api/izin-turleri", TestContext.Current.CancellationToken);
        return turler.EnumerateArray().Single(t => t.GetProperty("ad").GetString() == ad).GetProperty("id").GetInt32();
    }

    public static DateOnly SonrakiYilPazartesi() => MartinIlkPazartesi(DateTime.Today.Year + 1);

    public static DateOnly GecenYilPazartesi() => MartinIlkPazartesi(DateTime.Today.Year - 1);

    private static DateOnly MartinIlkPazartesi(int yil)
    {
        var gun = new DateOnly(yil, 3, 1);
        while (gun.DayOfWeek != DayOfWeek.Monday)
        {
            gun = gun.AddDays(1);
        }

        return gun;
    }

    private static async Task<int> IdOkuAsync(HttpResponseMessage yanit)
    {
        var govde = await yanit.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.True(yanit.IsSuccessStatusCode, govde);
        return JsonDocument.Parse(govde).RootElement.GetProperty("id").GetInt32();
    }
}
