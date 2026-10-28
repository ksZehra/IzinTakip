using System.Diagnostics;
using IzinTakip.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace IzinTakip.Api.Common.Errors;

public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public static string TraceId(HttpContext httpContext) =>
        Activity.Current?.TraceId.ToString() ?? httpContext.TraceIdentifier;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var status = exception switch
        {
            KimlikDogrulanamadiException => StatusCodes.Status401Unauthorized,
            IsKuraliException => StatusCodes.Status422UnprocessableEntity,
            KayitBulunamadiException => StatusCodes.Status404NotFound,
            YetkiYokException => StatusCodes.Status403Forbidden,
            CakismaException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        string detail;
        if (exception is IzinTakipException bilinenHata)
        {
            logger.LogWarning("Istek reddedildi {Durum}: {Mesaj}", status, bilinenHata.Message);
            detail = bilinenHata.Message;
        }
        else
        {
            logger.LogError(exception, "Beklenmeyen hata");
            detail = $"Beklenmeyen bir hata oluştu. Destek için bu kodu iletin: {TraceId(httpContext)}";
        }

        httpContext.Response.StatusCode = status;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails { Status = status, Detail = detail }
        });
    }
}
