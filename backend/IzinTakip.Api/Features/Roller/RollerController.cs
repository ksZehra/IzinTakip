using IzinTakip.Api.Common.Auth;
using IzinTakip.Domain;
using Microsoft.AspNetCore.Mvc;

namespace IzinTakip.Api.Features.Roller;

[ApiController]
[Route("api/roller")]
public sealed class RollerController(RolService service) : ControllerBase
{
    [HttpGet]
    [Permission(Yetkiler.Kullanici.Yonet)]
    [EndpointSummary("Rolleri yetkileri ve kullanıcı sayısıyla listeler (kullanıcı ekranındaki rol seçimi için)")]
    public async Task<ActionResult<IReadOnlyList<RolResponse>>> Listele(CancellationToken cancellationToken) =>
        Ok(await service.ListeleAsync(cancellationToken));

    [HttpGet("{id:int}")]
    [Permission(Yetkiler.Kullanici.Yonet)]
    [EndpointSummary("Rol detayını döner")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RolResponse>> Detay(int id, CancellationToken cancellationToken) =>
        await service.DetayAsync(id, cancellationToken);

    [HttpGet("yetkiler")]
    [Permission(Yetkiler.RolYonetimi)]
    [EndpointSummary("Rollere verilebilecek yetkileri açıklamalarıyla listeler")]
    public ActionResult<IReadOnlyList<YetkiTanimi>> AtanabilirYetkiler() => Ok(Yetkiler.Atanabilir);

    [HttpPost]
    [Permission(Yetkiler.RolYonetimi)]
    [EndpointSummary("Yeni rol açar (yalnız SuperAdmin)")]
    [ProducesResponseType<RolResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<RolResponse>> Olustur(RolKaydetRequest request, CancellationToken cancellationToken)
    {
        var sonuc = await service.OlusturAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Detay), new { id = sonuc.Id }, sonuc);
    }

    [HttpPut("{id:int}")]
    [Permission(Yetkiler.RolYonetimi)]
    [EndpointSummary("Rolün adını, korumalı işaretini ve yetkilerini günceller (yalnız SuperAdmin)")]
    public async Task<ActionResult<RolResponse>> Guncelle(int id, RolKaydetRequest request, CancellationToken cancellationToken) =>
        await service.GuncelleAsync(id, request, cancellationToken);

    [HttpDelete("{id:int}")]
    [Permission(Yetkiler.RolYonetimi)]
    [EndpointSummary("Kullanıcısı olmayan, sistem rolü olmayan rolü siler (yalnız SuperAdmin)")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Sil(int id, CancellationToken cancellationToken)
    {
        await service.SilAsync(id, cancellationToken);
        return NoContent();
    }
}
