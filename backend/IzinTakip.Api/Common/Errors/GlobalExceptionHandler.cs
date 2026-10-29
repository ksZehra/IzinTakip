using System.Diagnostics;
using System.Security.Claims;
using IzinTakip.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Serilog.Context;

namespace IzinTakip.Api.Common.Errors;

public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private const string CakismaMesaji = "Kayıt başka bir kullanıcı tarafından değiştirildi, sayfayı yenileyip tekrar deneyin.";
    private const string TekrarMesaji = "Aynı bilgilerle bir kayıt zaten var.";
    private const string EsZamanliIslemMesaji = "İşlem aynı anda yapılan başka bir işlemle çakıştı, tekrar deneyin.";

    public static string TraceId(HttpContext httpContext) =>
        Activity.Current?.TraceId.ToString() ?? httpContext.TraceIdentifier;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, bilinenMesaj) = exception switch
        {
            UnauthorizedException e => (StatusCodes.Status401Unauthorized, e.Message),
            BusinessRuleException e => (StatusCodes.Status422UnprocessableEntity, e.Message),
            NotFoundException e => (StatusCodes.Status404NotFound, e.Message),
            ForbiddenException e => (StatusCodes.Status403Forbidden, e.Message),
            DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, CakismaMesaji),
            _ when SqlHataNumarasi(exception) is 2601 or 2627 => (StatusCodes.Status409Conflict, TekrarMesaji),
            _ when SqlHataNumarasi(exception) is 1205 => (StatusCodes.Status409Conflict, EsZamanliIslemMesaji),
            _ => (StatusCodes.Status500InternalServerError, null)
        };

        string detail;
        using (LogContext.PushProperty("KullaniciId", httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)))
        {
            if (bilinenMesaj is not null)
            {
                logger.LogWarning("Istek reddedildi {Durum}: {Mesaj}", status, bilinenMesaj);
                detail = bilinenMesaj;
            }
            else
            {
                logger.LogError(exception, "Beklenmeyen hata");
                detail = $"Beklenmeyen bir hata oluştu. Destek için bu kodu iletin: {TraceId(httpContext)}";
            }
        }

        httpContext.Response.StatusCode = status;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails { Status = status, Detail = detail }
        });
    }

    private static int? SqlHataNumarasi(Exception exception)
    {
        for (var hata = exception; hata is not null; hata = hata.InnerException)
        {
            if (hata is SqlException sql)
            {
                return sql.Number;
            }
        }

        return null;
    }
}
