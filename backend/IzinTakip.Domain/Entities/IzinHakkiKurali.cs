using IzinTakip.Domain.Exceptions;

namespace IzinTakip.Domain.Entities;

public sealed class IzinHakkiKurali : Entity
{
    private IzinHakkiKurali()
    {
    }

    public IzinHakkiKurali(string ad, int minKidemYil, int? maxKidemYil, int? minYas, int? maxYas, int gun)
    {
        Aktif = true;
        Guncelle(ad, minKidemYil, maxKidemYil, minYas, maxYas, gun);
    }

    public string Ad { get; private set; } = string.Empty;
    public int MinKidemYil { get; private set; }
    public int? MaxKidemYil { get; private set; }
    public int? MinYas { get; private set; }
    public int? MaxYas { get; private set; }
    public int Gun { get; private set; }
    public bool Aktif { get; private set; }

    public void Guncelle(string ad, int minKidemYil, int? maxKidemYil, int? minYas, int? maxYas, int gun)
    {
        if (maxKidemYil < minKidemYil)
        {
            throw new IsKuraliException("En çok kıdem, en az kıdemden küçük olamaz.");
        }

        if (maxYas < minYas)
        {
            throw new IsKuraliException("En çok yaş, en az yaştan küçük olamaz.");
        }

        Ad = ad;
        MinKidemYil = minKidemYil;
        MaxKidemYil = maxKidemYil;
        MinYas = minYas;
        MaxYas = maxYas;
        Gun = gun;
    }

    public void AktiflikDegistir(bool aktif) => Aktif = aktif;
}
