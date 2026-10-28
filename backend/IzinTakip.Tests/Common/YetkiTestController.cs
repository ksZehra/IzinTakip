using IzinTakip.Api.Common.Auth;
using IzinTakip.Domain;
using Microsoft.AspNetCore.Mvc;

namespace IzinTakip.Tests.Common;

[ApiController]
[Route("test/yetki")]
public sealed class YetkiTestController : ControllerBase
{
    [HttpGet("rapor")]
    [Yetki(Yetkiler.Rapor.Gor)]
    public IActionResult Rapor() => Ok();

    [HttpGet("rol-yonetimi")]
    [Yetki(Yetkiler.RolYonetimi)]
    public IActionResult RolYonetimi() => Ok();
}
