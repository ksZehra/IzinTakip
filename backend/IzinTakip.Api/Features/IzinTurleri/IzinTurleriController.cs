using IzinTakip.Api.Common;
using IzinTakip.Api.Common.Auth;
using IzinTakip.Domain;
using Microsoft.AspNetCore.Mvc;

namespace IzinTakip.Api.Features.IzinTurleri;

[ApiController]
[Route("api/izin-turleri")]
public sealed class IzinTurleriController(IzinTuruService service) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("İzin türlerini listeler; aktif=true ile yalnız talep açılabilecek türler gelir")]
    public async Task<ActionResult<IReadOnlyList<IzinTuruResponse>>> Listele([FromQuery] bool? aktif, CancellationToken cancellationToken) =>
        Ok(await service.ListeleAsync(aktif, cancellationToken));

    [HttpGet("{id:int}")]
    [EndpointSummary("İzin türü detayını döner")]
    public async Task<ActionResult<IzinTuruResponse>> Detay(int id, CancellationToken cancellationToken) =>
        await service.DetayAsync(id, cancellationToken);

    [HttpPost]
    [Yetki(Yetkiler.Tanim.Yonet)]
    [EndpointSummary("Yeni izin türü ekler; süre sınırları dakikadır")]
    [ProducesResponseType<IzinTuruResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<IzinTuruResponse>> Olustur(IzinTuruKaydetRequest request, CancellationToken cancellationToken)
    {
        var sonuc = await service.OlusturAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Detay), new { id = sonuc.Id }, sonuc);
    }

    [HttpPut("{id:int}")]
    [Yetki(Yetkiler.Tanim.Yonet)]
    [EndpointSummary("İzin türünü günceller")]
    public async Task<ActionResult<IzinTuruResponse>> Guncelle(int id, IzinTuruKaydetRequest request, CancellationToken cancellationToken) =>
        await service.GuncelleAsync(id, request, cancellationToken);

    [HttpPut("{id:int}/aktiflik")]
    [Yetki(Yetkiler.Tanim.Yonet)]
    [EndpointSummary("İzin türünü aktif ya da pasif yapar")]
    public async Task<ActionResult<IzinTuruResponse>> AktiflikDegistir(int id, AktiflikRequest request, CancellationToken cancellationToken) =>
        await service.AktiflikDegistirAsync(id, request.Aktif, cancellationToken);
}
