using IzinTakip.Api.Common.Auth;
using IzinTakip.Domain;
using Microsoft.AspNetCore.Mvc;

namespace IzinTakip.Api.Features.ResmiTatiller;

[ApiController]
[Route("api/resmi-tatiller")]
public sealed class ResmiTatillerController(ResmiTatilService service) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Bir yılın resmi tatillerini listeler (yıl verilmezse bu yıl)")]
    public async Task<ActionResult<IReadOnlyList<ResmiTatilResponse>>> Listele([FromQuery] int? yil, CancellationToken cancellationToken) =>
        Ok(await service.ListeleAsync(yil, cancellationToken));

    [HttpGet("{id:int}")]
    [EndpointSummary("Resmi tatil detayını döner")]
    public async Task<ActionResult<ResmiTatilResponse>> Detay(int id, CancellationToken cancellationToken) =>
        await service.DetayAsync(id, cancellationToken);

    [HttpPost]
    [Yetki(Yetkiler.Tanim.Yonet)]
    [EndpointSummary("Resmi tatil ekler; yarimGun = true arife gibi öğleden sonrası tatil olan günlerdir")]
    [ProducesResponseType<ResmiTatilResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<ResmiTatilResponse>> Olustur(ResmiTatilKaydetRequest request, CancellationToken cancellationToken)
    {
        var sonuc = await service.OlusturAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Detay), new { id = sonuc.Id }, sonuc);
    }

    [HttpPut("{id:int}")]
    [Yetki(Yetkiler.Tanim.Yonet)]
    [EndpointSummary("Resmi tatili günceller")]
    public async Task<ActionResult<ResmiTatilResponse>> Guncelle(int id, ResmiTatilKaydetRequest request, CancellationToken cancellationToken) =>
        await service.GuncelleAsync(id, request, cancellationToken);

    [HttpDelete("{id:int}")]
    [Yetki(Yetkiler.Tanim.Yonet)]
    [EndpointSummary("Resmi tatili siler")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Sil(int id, CancellationToken cancellationToken)
    {
        await service.SilAsync(id, cancellationToken);
        return NoContent();
    }
}
