using FluentValidation;
using IzinTakip.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IzinTakip.Tests.Common;

[ApiController]
[AllowAnonymous]
[Route("test/hata")]
public sealed class HataTestController : ControllerBase
{
    [HttpGet("is-kurali")]
    public IActionResult IsKurali() => throw new IsKuraliException("Bakiyeniz 3 gün, talep 5 gün.");

    [HttpGet("bulunamadi")]
    public IActionResult Bulunamadi() => throw new KayitBulunamadiException("İzin talebi bulunamadı.");

    [HttpGet("eszamanlilik")]
    public IActionResult Eszamanlilik() => throw new DbUpdateConcurrencyException("Gizli teknik ayrinti: RowVersion");

    [HttpGet("beklenmeyen")]
    public IActionResult Beklenmeyen() => throw new InvalidOperationException("Gizli teknik ayrinti: Server=db01");

    [HttpPost("dogrulama")]
    public IActionResult Dogrulama(DogrulamaTestRequest request) => Ok(request);
}

public sealed record DogrulamaTestRequest(string? Ad);

public sealed class DogrulamaTestRequestValidator : AbstractValidator<DogrulamaTestRequest>
{
    public DogrulamaTestRequestValidator() => RuleFor(r => r.Ad).NotEmpty().WithMessage("Ad zorunludur.");
}
