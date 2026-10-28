namespace IzinTakip.Domain.Izin;

public sealed record Bakiye(int HakDakika, int KullanilanDakika, int BekleyenDakika)
{
    public int KullanilabilirDakika => HakDakika - KullanilanDakika - BekleyenDakika;
}
