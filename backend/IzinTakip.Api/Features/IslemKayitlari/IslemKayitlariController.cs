using IzinTakip.Api.Common.Auth;
using IzinTakip.Api.Common.Paging;
using IzinTakip.Domain;
using Microsoft.AspNetCore.Mvc;

namespace IzinTakip.Api.Features.IslemKayitlari;

[ApiController]
[Route("api/islem-kayitlari")]
[Yetki(Yetkiler.Denetim.Gor)]
public sealed class IslemKayitlariController(IslemKaydiService service) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Yönetim işlemlerinin kaydını (kim, ne zaman, hangi kaydı nasıl değiştirdi) yeniden eskiye sayfalı listeler")]
    public async Task<ActionResult<PagedResponse<IslemKaydiResponse>>> Listele(
        [FromQuery] IslemKaydiListeRequest request,
        CancellationToken cancellationToken) =>
        await service.ListeleAsync(request, cancellationToken);
}
