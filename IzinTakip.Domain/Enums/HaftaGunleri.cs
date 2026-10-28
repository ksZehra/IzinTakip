namespace IzinTakip.Domain.Enums;

[Flags]
public enum HaftaGunleri
{
    Yok = 0,
    Pazartesi = 1,
    Sali = 2,
    Carsamba = 4,
    Persembe = 8,
    Cuma = 16,
    Cumartesi = 32,
    Pazar = 64
}
