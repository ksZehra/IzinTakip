using IzinTakip.Api.Common.Auth;
using IzinTakip.Api.Common.Paging;
using IzinTakip.Domain;
using Microsoft.AspNetCore.Mvc;

namespace IzinTakip.Api.Features.IslemKayitlari;

[ApiController]
[Route("api/islem-kayitlari")]
[Permission(Yetkiler.Denetim.Gor)]
public sealed class IslemKayitlariController(IslemKaydiService service) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Yönetim işlemlerinin kaydını (kim, ne zaman, hangi kaydı nasıl değiştirdi) çoklu kayıt türü ve işlem, ilgili çalışan, tarih aralığıyla süzer; seçilen alana göre (varsayılan yeniden eskiye) sıralı ve sayfalı listeler")]
    public async Task<ActionResult<PagedResponse<IslemKaydiResponse>>> Listele(
        [FromQuery] IslemKaydiListeRequest request,
        CancellationToken cancellationToken) =>
        await service.ListeleAsync(request, cancellationToken);
}
