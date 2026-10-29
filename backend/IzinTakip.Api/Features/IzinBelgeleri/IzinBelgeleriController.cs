using IzinTakip.Api.Common.Auth;
using IzinTakip.Domain;
using Microsoft.AspNetCore.Mvc;

namespace IzinTakip.Api.Features.IzinBelgeleri;

[ApiController]
[Route("api/izin-talepleri/{talepId:int}/belgeler")]
[Permission(Yetkiler.IzinTalebi.Olustur, Yetkiler.IzinTalebi.Yonet)]
public sealed class IzinBelgeleriController(IzinBelgesiService service) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Kapsamdaki talebin belgelerini listeler")]
    public async Task<ActionResult<IReadOnlyList<IzinBelgesiResponse>>> Listele(int talepId, CancellationToken cancellationToken) =>
        Ok(await service.ListeleAsync(talepId, cancellationToken));

    [HttpPost]
    [Consumes("multipart/form-data")]
    [EndpointSummary("Talebe belge yükler (PDF, JPG, PNG; en çok 5 MB)")]
    [ProducesResponseType<IzinBelgesiResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<IzinBelgesiResponse>> Yukle(int talepId, [FromForm] IzinBelgesiYukleRequest request, CancellationToken cancellationToken)
    {
        var sonuc = await service.YukleAsync(talepId, request.Dosya, cancellationToken);
        return CreatedAtAction(nameof(Indir), new { talepId, id = sonuc.Id }, sonuc);
    }

    [HttpGet("{id:int}")]
    [EndpointSummary("Belgeyi indirir")]
    [ProducesResponseType<Stream>(StatusCodes.Status200OK, "application/pdf", "image/jpeg", "image/png")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Indir(int talepId, int id, CancellationToken cancellationToken)
    {
        var dosya = await service.IndirAsync(talepId, id, cancellationToken);
        return File(dosya.Icerik, dosya.IcerikTuru, dosya.DosyaAdi);
    }

    [HttpDelete("{id:int}")]
    [EndpointSummary("Beklemedeki talepten belgeyi siler")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Sil(int talepId, int id, CancellationToken cancellationToken)
    {
        await service.SilAsync(talepId, id, cancellationToken);
        return NoContent();
    }
}
