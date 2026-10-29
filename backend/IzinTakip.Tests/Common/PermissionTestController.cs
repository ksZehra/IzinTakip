using IzinTakip.Api.Common.Auth;
using IzinTakip.Domain;
using Microsoft.AspNetCore.Mvc;

namespace IzinTakip.Tests.Common;

[ApiController]
[Route("test/yetki")]
public sealed class PermissionTestController : ControllerBase
{
    [HttpGet("rapor")]
    [Permission(Yetkiler.Rapor.Gor)]
    public IActionResult Rapor() => Ok();

    [HttpGet("rol-yonetimi")]
    [Permission(Yetkiler.RolYonetimi)]
    public IActionResult RolYonetimi() => Ok();
}
