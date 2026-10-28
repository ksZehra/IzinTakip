using IzinTakip.Api.Common.Auth;
using IzinTakip.Api.Common.Paging;
using IzinTakip.Domain;
using Microsoft.AspNetCore.Mvc;

namespace IzinTakip.Api.Features.IzinTalepleri;

[ApiController]
[Route("api/izin-talepleri")]
[Yetki(Yetkiler.IzinTalebi.Olustur, Yetkiler.IzinTalebi.Yonet)]
public sealed class IzinTalepleriController(IzinTalebiService service) : ControllerBase
{
    [HttpPost]
    [EndpointSummary("İzin talebi açar; calisanId boşsa kendi adına, doluysa kapsamdaki çalışan adına")]
    [ProducesResponseType<IzinTalebiResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<IzinTalebiResponse>> Olustur(IzinTalebiOlusturRequest request, CancellationToken cancellationToken)
    {
        var sonuc = await service.OlusturAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Detay), new { id = sonuc.Id }, sonuc);
    }

    [HttpPost("sure-hesapla")]
    [EndpointSummary("Talebi kaydetmeden süresini hesaplar (form önizlemesi)")]
    public async Task<ActionResult<SureHesaplaResponse>> SureHesapla(SureHesaplaRequest request, CancellationToken cancellationToken) =>
        await service.SureHesaplaAsync(request, cancellationToken);

    [HttpGet]
    [EndpointSummary("Kapsamdaki izin taleplerini çalışan, durum, tür ve tarih aralığıyla sayfalı listeler")]
    public async Task<ActionResult<PagedResponse<IzinTalebiListeResponse>>> Listele(
        [FromQuery] IzinTalebiListeRequest request,
        CancellationToken cancellationToken) =>
        await service.ListeleAsync(request, cancellationToken);

    [HttpGet("{id:int}")]
    [EndpointSummary("Kapsamdaki bir izin talebinin detayını döner")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IzinTalebiResponse>> Detay(int id, CancellationToken cancellationToken) =>
        await service.DetayAsync(id, cancellationToken);
}
