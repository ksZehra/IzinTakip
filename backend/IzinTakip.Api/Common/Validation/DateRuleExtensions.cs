using FluentValidation;

namespace IzinTakip.Api.Common.Validation;

public static class DateRuleExtensions
{
    public static readonly DateOnly EnErken = new(2000, 1, 1);
    public static readonly DateOnly EnGec = new(2100, 12, 31);

    private const string Mesaj = "Geçerli bir tarih girin.";

    public static IRuleBuilderOptions<T, DateOnly> GecerliTarih<T>(this IRuleBuilder<T, DateOnly> kural) =>
        kural.InclusiveBetween(EnErken, EnGec).WithMessage(Mesaj);

    public static IRuleBuilderOptions<T, DateOnly?> GecerliTarih<T>(this IRuleBuilder<T, DateOnly?> kural) =>
        kural.Must(tarih => tarih is null || (tarih >= EnErken && tarih <= EnGec)).WithMessage(Mesaj);
}
