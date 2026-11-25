using FluentValidation;
using IzinTakip.Api.Common.Paging;

namespace IzinTakip.Api.Common.Validation;

public static class SiralamaRuleExtensions
{
    public static IRuleBuilderOptions<T, string?> GecerliSiralama<T>(this IRuleBuilder<T, string?> kural, IEnumerable<string> alanlar) =>
        kural
            .Must(sort => string.IsNullOrEmpty(sort) || (SiralamaIstegi.Coz(sort) is { } istek && alanlar.Contains(istek.Alan)))
            .WithMessage("Geçerli bir sıralama seçin.");
}
