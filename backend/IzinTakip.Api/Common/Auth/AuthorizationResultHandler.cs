using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc;

namespace IzinTakip.Api.Common.Auth;

public sealed class AuthorizationResultHandler(IProblemDetailsService problemDetailsService) : IAuthorizationMiddlewareResultHandler
{
    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Succeeded)
        {
            await next(context);
            return;
        }

        int status;
        string detail;
        if (authorizeResult.Challenged)
        {
            await context.ChallengeAsync();
            status = StatusCodes.Status401Unauthorized;
            detail = "Oturum açmanız gerekiyor ya da oturumunuzun süresi doldu.";
        }
        else
        {
            await context.ForbidAsync();
            status = StatusCodes.Status403Forbidden;
            detail = authorizeResult.AuthorizationFailure?.FailureReasons.FirstOrDefault()?.Message ?? "Bu işlem için yetkiniz yok.";
        }

        context.Response.StatusCode = status;
        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = new ProblemDetails { Status = status, Detail = detail }
        });
    }
}
