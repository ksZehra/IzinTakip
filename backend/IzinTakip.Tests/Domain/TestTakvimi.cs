using IzinTakip.Domain.Enums;
using IzinTakip.Domain.Izin;

namespace IzinTakip.Tests.Domain;

public static class TestTakvimi
{
    public const HaftaGunleri HaftaIci = HaftaGunleri.Pazartesi | HaftaGunleri.Sali | HaftaGunleri.Carsamba | HaftaGunleri.Persembe | HaftaGunleri.Cuma;

    public static readonly DateOnly Pazartesi = new(2026, 3, 2);
    public static readonly DateOnly Arife = new(2026, 10, 28);
    public static readonly DateOnly CumhuriyetBayrami = new(2026, 10, 29);

    public static CalismaTakvimi Olustur() => new(
        HaftaIci,
        new TimeOnly(9, 0),
        new TimeOnly(18, 0),
        new TimeOnly(12, 30),
        new TimeOnly(13, 30),
        30,
        [(Arife, true), (CumhuriyetBayrami, false)]);
}
