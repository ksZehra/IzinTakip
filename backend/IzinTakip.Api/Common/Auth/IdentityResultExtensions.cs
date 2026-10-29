using IzinTakip.Domain.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace IzinTakip.Api.Common.Auth;

public static class IdentityResultExtensions
{
    public static async Task DogrulaAsync(this Task<IdentityResult> islem)
    {
        var sonuc = await islem;
        if (!sonuc.Succeeded)
        {
            throw new BusinessRuleException(string.Join(" ", sonuc.Errors.Select(e => e.Description).Distinct()));
        }
    }
}
