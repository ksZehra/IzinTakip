using System.Diagnostics;
using IzinTakip.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

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
            KimlikDogrulanamadiException e => (StatusCodes.Status401Unauthorized, e.Message),
            IsKuraliException e => (StatusCodes.Status422UnprocessableEntity, e.Message),
            KayitBulunamadiException e => (StatusCodes.Status404NotFound, e.Message),
            YetkiYokException e => (StatusCodes.Status403Forbidden, e.Message),
            DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, CakismaMesaji),
            DbUpdateException { InnerException: SqlException { Number: 2601 or 2627 } } => (StatusCodes.Status409Conflict, TekrarMesaji),
            SqlException { Number: 1205 } or DbUpdateException { InnerException: SqlException { Number: 1205 } } => (StatusCodes.Status409Conflict, EsZamanliIslemMesaji),
            _ => (StatusCodes.Status500InternalServerError, null)
        };

        string detail;
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

        httpContext.Response.StatusCode = status;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails { Status = status, Detail = detail }
        });
    }
}
