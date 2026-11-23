using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace IzinTakip.Gateway.Bff;

[ApiController]
[Route("bff")]
public sealed class BffController(IHttpClientFactory httpClientFactory, OturumCerezi oturumCerezi, ILogger<BffController> logger) : ControllerBase
{
    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
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

        var login = await yanit.Content.ReadFromJsonAsync<ApiLoginResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Api login yaniti okunamadi.");
        oturumCerezi.Yaz(Response, login.AccessToken, new DateTimeOffset(login.SureBitis, TimeSpan.Zero));

        logger.LogInformation("Oturum acildi");
        return NoContent();
    }

    [HttpPost("cikis")]
    public async Task<IActionResult> Cikis(CancellationToken cancellationToken)
    {
        if (oturumCerezi.Oku(Request) is { } token)
        {
            using var istek = new HttpRequestMessage(HttpMethod.Post, "api/auth/cikis");
            istek.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            try
            {
                using var yanit = await httpClientFactory.CreateClient("api").SendAsync(istek, cancellationToken);
                if (!yanit.IsSuccessStatusCode)
                {
                    logger.LogInformation("Cikista oturum Api'de kapatilamadi {Durum}", (int)yanit.StatusCode);
                }
            }
            catch (HttpRequestException exception)
            {
                logger.LogWarning(exception, "Cikista Api'ye ulasilamadi; cookie yine de silindi");
            }
        }

        oturumCerezi.Sil(Response);
        return NoContent();
    }
}
