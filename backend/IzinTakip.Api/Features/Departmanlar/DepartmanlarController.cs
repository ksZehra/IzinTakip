using IzinTakip.Api.Common;
using IzinTakip.Api.Common.Auth;
using IzinTakip.Domain;
using Microsoft.AspNetCore.Mvc;

namespace IzinTakip.Api.Features.Departmanlar;

[ApiController]
[Route("api/departmanlar")]
public sealed class DepartmanlarController(DepartmanService service) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Departmanları yöneticisi ve aktif çalışan sayısıyla listeler")]
    public async Task<ActionResult<IReadOnlyList<DepartmanResponse>>> Listele(CancellationToken cancellationToken) =>
        Ok(await service.ListeleAsync(cancellationToken));

    [HttpGet("{id:int}")]
    [EndpointSummary("Departman detayını döner")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DepartmanResponse>> Detay(int id, CancellationToken cancellationToken) =>
        await service.DetayAsync(id, cancellationToken);

    [HttpPost]
    [Yetki(Yetkiler.Tanim.Yonet)]
    [EndpointSummary("Yeni departman ekler")]
    [ProducesResponseType<DepartmanResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<DepartmanResponse>> Olustur(DepartmanKaydetRequest request, CancellationToken cancellationToken)
    {
        var sonuc = await service.OlusturAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Detay), new { id = sonuc.Id }, sonuc);
    }

    [HttpPut("{id:int}")]
    [Yetki(Yetkiler.Tanim.Yonet)]
    [EndpointSummary("Departmanın adını ve yöneticisini günceller")]
    public async Task<ActionResult<DepartmanResponse>> Guncelle(int id, DepartmanKaydetRequest request, CancellationToken cancellationToken) =>
        await service.GuncelleAsync(id, request, cancellationToken);

    [HttpPut("{id:int}/aktiflik")]
    [Yetki(Yetkiler.Tanim.Yonet)]
    [EndpointSummary("Departmanı aktif ya da pasif yapar")]
    public async Task<ActionResult<DepartmanResponse>> AktiflikDegistir(int id, AktiflikRequest request, CancellationToken cancellationToken) =>
        await service.AktiflikDegistirAsync(id, request.Aktif, cancellationToken);
}
