using IzinTakip.Api.Common.Paging;
using Microsoft.AspNetCore.Mvc;

namespace IzinTakip.Api.Features.Bildirimler;

[ApiController]
[Route("api/bildirimler")]
public sealed class BildirimlerController(BildirimService service) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Oturumdaki kullanıcının bildirimlerini yeniden eskiye sayfalı listeler")]
    public async Task<ActionResult<PagedResponse<BildirimResponse>>> Listele(
        [FromQuery] BildirimListeRequest request,
        CancellationToken cancellationToken) =>
        await service.ListeleAsync(request, cancellationToken);

    [HttpGet("sayi")]
    [EndpointSummary("Okunmamış bildirim sayısını döner (zil ikonu)")]
    public async Task<ActionResult<BildirimSayisiResponse>> Sayi(CancellationToken cancellationToken) =>
        await service.SayiAsync(cancellationToken);

    [HttpPut("{id:int}/okundu")]
    [EndpointSummary("Bir bildirimi okundu işaretler")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> OkunduIsaretle(int id, CancellationToken cancellationToken)
    {
        await service.OkunduIsaretleAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPut("okundu")]
    [EndpointSummary("Bütün bildirimleri okundu işaretler")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> TumunuOkunduIsaretle(CancellationToken cancellationToken)
    {
        await service.TumunuOkunduIsaretleAsync(cancellationToken);
        return NoContent();
    }
}
