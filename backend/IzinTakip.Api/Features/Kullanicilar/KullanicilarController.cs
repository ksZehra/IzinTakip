using IzinTakip.Api.Common;
using IzinTakip.Api.Common.Auth;
using IzinTakip.Api.Common.Paging;
using IzinTakip.Domain;
using Microsoft.AspNetCore.Mvc;

namespace IzinTakip.Api.Features.Kullanicilar;

[ApiController]
[Route("api/kullanicilar")]
[Permission(Yetkiler.Kullanici.Yonet)]
public sealed class KullanicilarController(KullaniciService service) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Kullanıcıları e-posta ya da çalışan adıyla arar, sayfalı listeler")]
    public async Task<ActionResult<PagedResponse<KullaniciResponse>>> Listele(
        [FromQuery] KullaniciListeRequest request,
        CancellationToken cancellationToken) =>
        await service.ListeleAsync(request, cancellationToken);

    [HttpGet("{id:int}")]
    [EndpointSummary("Kullanıcı detayını rolleriyle döner")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<KullaniciResponse>> Detay(int id, CancellationToken cancellationToken) =>
        await service.DetayAsync(id, cancellationToken);

    [HttpPost]
    [EndpointSummary("Hesap açar; geçici parola e-postayla gider, ilk girişte değiştirilir. Korumalı rolü yalnız SuperAdmin atar")]
    [ProducesResponseType<KullaniciResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<KullaniciResponse>> Olustur(KullaniciOlusturRequest request, CancellationToken cancellationToken)
    {
        var sonuc = await service.OlusturAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Detay), new { id = sonuc.Id }, sonuc);
    }

    [HttpPut("{id:int}/roller")]
    [EndpointSummary("Kullanıcının rollerini verilen listeyle değiştirir")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<KullaniciResponse>> RolleriDegistir(int id, KullaniciRolleriRequest request, CancellationToken cancellationToken) =>
        await service.RolleriDegistirAsync(id, request, cancellationToken);

    [HttpPut("{id:int}/aktiflik")]
    [EndpointSummary("Hesabı aktif ya da pasif yapar")]
    public async Task<ActionResult<KullaniciResponse>> AktiflikDegistir(int id, AktiflikRequest request, CancellationToken cancellationToken) =>
        await service.AktiflikDegistirAsync(id, request.Aktif, cancellationToken);

    [HttpPost("{id:int}/parola-sifirla")]
    [EndpointSummary("Geçici parola üretir, e-postayla gönderir ve ilk girişte değiştirmeyi zorunlu kılar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ParolaSifirla(int id, CancellationToken cancellationToken)
    {
        await service.ParolaSifirlaAsync(id, cancellationToken);
        return NoContent();
    }
}
