using IzinTakip.Api.Common;
using IzinTakip.Api.Common.Auth;
using IzinTakip.Domain;
using Microsoft.AspNetCore.Mvc;

namespace IzinTakip.Api.Features.HakKurallari;

[ApiController]
[Route("api/hak-kurallari")]
[Permission(Yetkiler.Tanim.Yonet)]
public sealed class HakKurallariController(HakKuraliService service) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Yıllık izin hak kurallarını listeler")]
    public async Task<ActionResult<IReadOnlyList<HakKuraliResponse>>> Listele(CancellationToken cancellationToken) =>
        Ok(await service.ListeleAsync(cancellationToken));

    [HttpGet("{id:int}")]
    [EndpointSummary("Hak kuralı detayını döner")]
    public async Task<ActionResult<HakKuraliResponse>> Detay(int id, CancellationToken cancellationToken) =>
        await service.DetayAsync(id, cancellationToken);

    [HttpPost]
    [EndpointSummary("Yeni hak kuralı ekler (kıdem aralığı, isteğe bağlı yaş koşulu, gün)")]
    [ProducesResponseType<HakKuraliResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<HakKuraliResponse>> Olustur(HakKuraliKaydetRequest request, CancellationToken cancellationToken)
    {
        var sonuc = await service.OlusturAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Detay), new { id = sonuc.Id }, sonuc);
    }

    [HttpPut("{id:int}")]
    [EndpointSummary("Hak kuralını günceller; geçmişte yazılmış haklar değişmez")]
    public async Task<ActionResult<HakKuraliResponse>> Guncelle(int id, HakKuraliKaydetRequest request, CancellationToken cancellationToken) =>
        await service.GuncelleAsync(id, request, cancellationToken);

    [HttpPut("{id:int}/aktiflik")]
    [EndpointSummary("Hak kuralını aktif ya da pasif yapar")]
    public async Task<ActionResult<HakKuraliResponse>> AktiflikDegistir(int id, AktiflikRequest request, CancellationToken cancellationToken) =>
        await service.AktiflikDegistirAsync(id, request.Aktif, cancellationToken);
}
