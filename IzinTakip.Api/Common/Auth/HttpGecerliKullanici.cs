using System.Security.Claims;
using IzinTakip.Infrastructure.Identity;

namespace IzinTakip.Api.Common.Auth;

public sealed class HttpGecerliKullanici(IHttpContextAccessor httpContextAccessor) : IGecerliKullanici
{
    public int? KullaniciId =>
        int.TryParse(httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
}
