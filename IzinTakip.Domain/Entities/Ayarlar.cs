using IzinTakip.Domain.Enums;

namespace IzinTakip.Domain.Entities;

public sealed class Ayarlar : Entity
{
    public const int TekSatirId = 1;

    private Ayarlar()
    {
    }

    public HaftaGunleri CalismaGunleri { get; private set; }
    public TimeOnly MesaiBaslangic { get; private set; }
    public TimeOnly MesaiBitis { get; private set; }
    public TimeOnly OgleArasiBaslangic { get; private set; }
    public TimeOnly OgleArasiBitis { get; private set; }
    public int SaatlikIzinAdimDakika { get; private set; }
    public int HatirlatmaGunSayisi { get; private set; }
}
