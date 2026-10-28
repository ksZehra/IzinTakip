using IzinTakip.Api.Common.Auth;
using IzinTakip.Api.Common.Paging;
using IzinTakip.Domain;
using Microsoft.AspNetCore.Mvc;

namespace IzinTakip.Api.Features.IzinTalepleri;

[ApiController]
[Route("api/izin-talepleri")]
[Yetki(Yetkiler.IzinTalebi.Olustur, Yetkiler.IzinTalebi.Yonet)]
public sealed class IzinTalepleriController(IzinTalebiService service, IzinOnayService onayService) : ControllerBase
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

    [HttpGet("excel")]
    [Yetki(Yetkiler.Rapor.Gor)]
    [EndpointSummary("Kapsamdaki izin taleplerini liste filtreleriyle Excel dosyası olarak indirir")]
    [ProducesResponseType<Stream>(StatusCodes.Status200OK, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    public async Task<IActionResult> Excel([FromQuery] IzinTalebiFiltreRequest request, CancellationToken cancellationToken)
    {
        var dosya = await service.ExcelAsync(request, cancellationToken);
        return File(dosya.Icerik, dosya.IcerikTuru, dosya.DosyaAdi);
    }

    [HttpGet("{id:int}")]
    [EndpointSummary("Kapsamdaki bir izin talebinin detayını döner")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IzinTalebiResponse>> Detay(int id, CancellationToken cancellationToken) =>
        await service.DetayAsync(id, cancellationToken);

    [HttpPost("{id:int}/onayla")]
    [Yetki(Yetkiler.IzinTalebi.Yonet)]
    [EndpointSummary("Beklemedeki talebi onaylar; kimse kendi talebini onaylayamaz (SuperAdmin hariç)")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<IzinTalebiResponse>> Onayla(int id, IzinTalebiKararRequest request, CancellationToken cancellationToken)
    {
        await onayService.OnaylaAsync(id, request.Not, cancellationToken);
        return await service.DetayAsync(id, cancellationToken);
    }

    [HttpPost("{id:int}/reddet")]
    [Yetki(Yetkiler.IzinTalebi.Yonet)]
    [EndpointSummary("Beklemedeki talebi gerekçesiyle reddeder")]
    public async Task<ActionResult<IzinTalebiResponse>> Reddet(int id, IzinTalebiReddetRequest request, CancellationToken cancellationToken)
    {
        await onayService.ReddetAsync(id, request.Gerekce, cancellationToken);
        return await service.DetayAsync(id, cancellationToken);
    }

    [HttpPost("{id:int}/iptal")]
    [EndpointSummary("Beklemedeki ya da onaylı talebi başlangıçtan önce iptal eder; başlamış izni yalnız İK iptal edebilir")]
    public async Task<ActionResult<IzinTalebiResponse>> IptalEt(int id, IzinTalebiKararRequest request, CancellationToken cancellationToken)
    {
        await onayService.IptalEtAsync(id, request.Not, cancellationToken);
        return await service.DetayAsync(id, cancellationToken);
    }
}
