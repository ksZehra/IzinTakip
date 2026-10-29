using FluentValidation;

namespace IzinTakip.Api.Common.Validation;

public static class SaatKurallari
{
    private const string Mesaj = "Saati saniyesiz girin (ör. 10:30).";

    public static IRuleBuilderOptions<T, TimeOnly> TamDakika<T>(this IRuleBuilder<T, TimeOnly> kural) =>
        kural.Must(saat => saat.Ticks % TimeSpan.TicksPerMinute == 0).WithMessage(Mesaj);

    public static IRuleBuilderOptions<T, TimeOnly?> TamDakika<T>(this IRuleBuilder<T, TimeOnly?> kural) =>
        kural.Must(saat => saat is null || saat.Value.Ticks % TimeSpan.TicksPerMinute == 0).WithMessage(Mesaj);
}
