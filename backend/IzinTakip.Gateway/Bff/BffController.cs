using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace IzinTakip.Gateway.Bff;

[ApiController]
[Route("bff")]
public sealed class BffController(IHttpClientFactory httpClientFactory, OturumCerezi oturumCerezi, ILogger<BffController> logger) : ControllerBase
{
    [HttpPost("giris")]
    [EnableRateLimiting("giris")]
    public async Task<IActionResult> Giris(GirisRequest request, CancellationToken cancellationToken)
    {
        using var yanit = await httpClientFactory.CreateClient("api").PostAsJsonAsync("api/auth/login", request, cancellationToken);
        if (!yanit.IsSuccessStatusCode)
        {
            return new ContentResult
            {
                StatusCode = (int)yanit.StatusCode,
                Content = await yanit.Content.ReadAsStringAsync(cancellationToken),
                ContentType = yanit.Content.Headers.ContentType?.ToString()
            };
        }

        var giris = await yanit.Content.ReadFromJsonAsync<ApiGirisResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Api giris yaniti okunamadi.");
        oturumCerezi.Yaz(Response, giris.AccessToken, new DateTimeOffset(giris.SureBitis, TimeSpan.Zero));

        logger.LogInformation("Oturum acildi");
        return NoContent();
    }

    [HttpPost("cikis")]
    public IActionResult Cikis()
    {
        oturumCerezi.Sil(Response);
        return NoContent();
    }
}
