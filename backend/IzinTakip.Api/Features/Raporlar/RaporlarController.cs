using IzinTakip.Api.Common.Auth;
using IzinTakip.Api.Common.Paging;
using IzinTakip.Domain;
using Microsoft.AspNetCore.Mvc;

namespace IzinTakip.Api.Features.Raporlar;

[ApiController]
[Route("api/raporlar")]
public sealed class RaporlarController(RaporService service) : ControllerBase
{
    [HttpGet("takvim")]
    [Permission(Yetkiler.IzinTalebi.Olustur, Yetkiler.IzinTalebi.Yonet)]
    [EndpointSummary("Kapsamdaki beklemede ve onaylı izinleri tarih aralığında listeler (en çok 3 ay)")]
    public async Task<ActionResult<IReadOnlyList<TakvimKaydiResponse>>> Takvim(
        [FromQuery] TakvimRequest request,
        CancellationToken cancellationToken) =>
        Ok(await service.TakvimAsync(request, cancellationToken));

    [HttpGet("bakiyeler")]
    [Permission(Yetkiler.Rapor.Gor)]
    [EndpointSummary("Kapsamdaki aktif çalışanların bakiyelerini sayfalı listeler")]
    public async Task<ActionResult<PagedResponse<BakiyeRaporuResponse>>> Bakiyeler(
        [FromQuery] BakiyeRaporuRequest request,
        CancellationToken cancellationToken) =>
        await service.BakiyelerAsync(request, cancellationToken);

    [HttpGet("bakiyeler/excel")]
    [Permission(Yetkiler.Rapor.Gor)]
    [EndpointSummary("Bakiye raporunu Excel dosyası olarak indirir")]
    [ProducesResponseType<Stream>(StatusCodes.Status200OK, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    public async Task<IActionResult> BakiyeExcel([FromQuery] int? departmanId, CancellationToken cancellationToken)
    {
        var dosya = await service.BakiyeExcelAsync(departmanId, cancellationToken);
        return File(dosya.Icerik, dosya.IcerikTuru, dosya.DosyaAdi);
    }
}
