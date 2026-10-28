using IzinTakip.Api.Common;
using IzinTakip.Api.Common.Auth;
using IzinTakip.Api.Common.Paging;
using IzinTakip.Domain;
using Microsoft.AspNetCore.Mvc;

namespace IzinTakip.Api.Features.Calisanlar;

[ApiController]
[Route("api/calisanlar")]
public sealed class CalisanlarController(CalisanService service) : ControllerBase
{
    [HttpGet]
    [Yetki(Yetkiler.Calisan.Yonet, Yetkiler.IzinTalebi.Yonet)]
    [EndpointSummary("Kapsamdaki çalışanları arama, departman ve aktiflik filtresiyle sayfalı listeler")]
    public async Task<ActionResult<PagedResponse<CalisanListeResponse>>> Listele(
        [FromQuery] CalisanListeRequest request,
        CancellationToken cancellationToken) =>
        await service.ListeleAsync(request, cancellationToken);

    [HttpGet("{id:int}")]
    [Yetki(Yetkiler.Calisan.Yonet, Yetkiler.IzinTalebi.Yonet, Yetkiler.IzinTalebi.Olustur)]
    [EndpointSummary("Kapsamdaki bir çalışanın detayını döner")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CalisanResponse>> Detay(int id, CancellationToken cancellationToken) =>
        await service.DetayAsync(id, cancellationToken);

    [HttpPost]
    [Yetki(Yetkiler.Calisan.Yonet)]
    [EndpointSummary("Yeni çalışan ekler")]
    [ProducesResponseType<CalisanResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<CalisanResponse>> Olustur(CalisanKaydetRequest request, CancellationToken cancellationToken)
    {
        var sonuc = await service.OlusturAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Detay), new { id = sonuc.Id }, sonuc);
    }

    [HttpPut("{id:int}")]
    [Yetki(Yetkiler.Calisan.Yonet)]
    [EndpointSummary("Çalışan bilgilerini günceller")]
    public async Task<ActionResult<CalisanResponse>> Guncelle(int id, CalisanKaydetRequest request, CancellationToken cancellationToken) =>
        await service.GuncelleAsync(id, request, cancellationToken);

    [HttpPut("{id:int}/aktiflik")]
    [Yetki(Yetkiler.Calisan.Yonet)]
    [EndpointSummary("Çalışanı aktif ya da pasif yapar; pasifleşen çalışanın kullanıcı hesabı da kapanır")]
    public async Task<ActionResult<CalisanResponse>> AktiflikDegistir(int id, AktiflikRequest request, CancellationToken cancellationToken) =>
        await service.AktiflikDegistirAsync(id, request.Aktif, cancellationToken);

    [HttpGet("{id:int}/haklar")]
    [Yetki(Yetkiler.Calisan.Yonet, Yetkiler.IzinTalebi.Yonet, Yetkiler.IzinTalebi.Olustur)]
    [EndpointSummary("Çalışanın hak satırlarını ve toplam hakkını döner")]
    public async Task<ActionResult<IzinHaklariResponse>> Haklar(int id, CancellationToken cancellationToken) =>
        await service.HaklariAsync(id, cancellationToken);

    [HttpPost("{id:int}/haklar")]
    [Yetki(Yetkiler.Calisan.Yonet)]
    [EndpointSummary("Çalışana elle hak satırı ekler (açılış bakiyesi ya da düzeltme; süre dakika, eksi olabilir)")]
    public async Task<ActionResult<IzinHaklariResponse>> HakEkle(int id, IzinHakkiEkleRequest request, CancellationToken cancellationToken) =>
        await service.HakEkleAsync(id, request, cancellationToken);
}
