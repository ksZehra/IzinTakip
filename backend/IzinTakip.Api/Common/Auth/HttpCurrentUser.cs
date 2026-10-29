using System.Security.Claims;
using IzinTakip.Domain.Exceptions;
using IzinTakip.Infrastructure.Identity;
using Microsoft.IdentityModel.JsonWebTokens;

namespace IzinTakip.Api.Common.Auth;

public sealed class HttpCurrentUser(IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider) : ICurrentUser
{
    public int? KullaniciId =>
        int.TryParse(httpContextAccessor.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.Sub), out var id) ? id : null;

    public async Task<KullaniciYetkileri> YetkileriAsync(CancellationToken cancellationToken)
    {
        if (KullaniciId is { } id
            && await serviceProvider.GetRequiredService<KullaniciYetkiProvider>().OkuAsync(id, cancellationToken) is { } yetkiler)
        {
            return yetkiler;
        }

        throw new UnauthorizedException("Oturum açmanız gerekiyor.");
    }
}
