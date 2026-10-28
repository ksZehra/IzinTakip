using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IzinTakip.Api.Features.Auth;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(AuthService service) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [EndpointSummary("E-posta ve parolayla giriş yapar, JWT access token döner")]
    [ProducesResponseType<GirisResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<GirisResponse>> Giris(GirisRequest request) =>
        await service.GirisAsync(request);

    [HttpGet("ben")]
    [Authorize]
    [EndpointSummary("Oturumdaki kullanıcının bilgisini, rollerini ve yetkilerini döner")]
    [ProducesResponseType<BenResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<BenResponse>> Ben(CancellationToken cancellationToken) =>
        await service.BenAsync(cancellationToken);

    [HttpPost("parola-degistir")]
    [Authorize]
    [EndpointSummary("Oturumdaki kullanıcının parolasını değiştirir")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ParolaDegistir(ParolaDegistirRequest request, CancellationToken cancellationToken)
    {
        await service.ParolaDegistirAsync(request, cancellationToken);
        return NoContent();
    }
}
