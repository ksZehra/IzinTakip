using IzinTakip.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;

namespace IzinTakip.Api.Common.Auth;

public sealed class YetkiAuthorizationHandler(IGecerliKullanici gecerliKullanici) : AuthorizationHandler<YetkiAttribute>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, YetkiAttribute requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var yetkiler = await gecerliKullanici.YetkileriAsync(CancellationToken.None);
        if (yetkiler.ParolaDegistirmeli)
        {
            context.Fail(new AuthorizationFailureReason(this, "Devam etmek için önce parolanızı değiştirin."));
            return;
        }

        if (requirement.Kodlar.Any(yetkiler.VarMi))
        {
            context.Succeed(requirement);
            return;
        }

        context.Fail(new AuthorizationFailureReason(this, "Bu işlem için yetkiniz yok."));
    }
}
