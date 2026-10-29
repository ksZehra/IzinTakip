using IzinTakip.Api.Common.Auth;
using IzinTakip.Domain;
using Microsoft.AspNetCore.Mvc;

namespace IzinTakip.Api.Features.Ayarlar;

[ApiController]
[Route("api/ayarlar")]
public sealed class AyarlarController(AyarlarService service) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("İş takvimi ayarlarını ve günlük çalışma süresini (dakika) döner")]
    public async Task<ActionResult<AyarlarResponse>> Getir(CancellationToken cancellationToken) =>
        await service.GetirAsync(cancellationToken);

    [HttpPut]
    [Permission(Yetkiler.Tanim.Yonet)]
    [EndpointSummary("Çalışma günlerini, mesai ve öğle arası saatlerini, saatlik izin adımını ve hatırlatma süresini günceller")]
    public async Task<ActionResult<AyarlarResponse>> Guncelle(AyarlarGuncelleRequest request, CancellationToken cancellationToken) =>
        await service.GuncelleAsync(request, cancellationToken);
}
