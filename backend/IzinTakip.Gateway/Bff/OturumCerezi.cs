using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;

namespace IzinTakip.Gateway.Bff;

public sealed class OturumCerezi(IDataProtectionProvider dataProtectionProvider)
{
    public const string Ad = "__Host-IzinTakip";

    private readonly IDataProtector _koruyucu = dataProtectionProvider.CreateProtector("IzinTakip.Gateway.Oturum");

    public void Yaz(HttpResponse response, string token, DateTimeOffset bitis) =>
        response.Cookies.Append(Ad, _koruyucu.Protect(token), Secenekler(bitis));

    public void Sil(HttpResponse response) => response.Cookies.Delete(Ad, Secenekler(null));

    public string? Oku(HttpRequest request)
    {
        if (!request.Cookies.TryGetValue(Ad, out var deger))
        {
            return null;
        }

        try
        {
            return _koruyucu.Unprotect(deger);
        }
        catch (CryptographicException)
        {
            return null;
        }
    }

    private static CookieOptions Secenekler(DateTimeOffset? bitis) => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Path = "/",
        Expires = bitis,
        IsEssential = true
    };
}
