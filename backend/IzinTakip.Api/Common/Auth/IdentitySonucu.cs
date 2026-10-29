using IzinTakip.Domain.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace IzinTakip.Api.Common.Auth;

public static class IdentitySonucu
{
    public static void Dogrula(IdentityResult sonuc)
    {
        if (!sonuc.Succeeded)
        {
            throw new IsKuraliException(string.Join(" ", sonuc.Errors.Select(e => e.Description).Distinct()));
        }
    }
}
