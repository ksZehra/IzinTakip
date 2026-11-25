using FluentValidation;

namespace IzinTakip.Api.Common.Validation;

public static class SecimRuleExtensions
{
    public const int EnFazlaSecim = 100;

    public static IRuleBuilderOptions<T, TEleman[]?> SinirliSecim<T, TEleman>(this IRuleBuilder<T, TEleman[]?> kural) =>
        kural
            .Must(secim => secim is null || secim.Length <= EnFazlaSecim)
            .WithMessage($"En çok {EnFazlaSecim} değer seçilebilir.");
}
