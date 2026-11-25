using IzinTakip.Domain.Enums;
using IzinTakip.Domain.Exceptions;

namespace IzinTakip.Domain.Entities;

public sealed class IzinHakki : Entity
{
    private IzinHakki()
    {
    }

    private IzinHakki(int calisanId, int? kidemYili, DateOnly kazanimTarihi, int dakika, HakKaynagi kaynak, string aciklama)
    {
        CalisanId = calisanId;
        KidemYili = kidemYili;
        KazanimTarihi = kazanimTarihi;
        Dakika = dakika;
        Kaynak = kaynak;
        Aciklama = aciklama;
    }

    public int CalisanId { get; private set; }
    public int? KidemYili { get; private set; }
    public DateOnly KazanimTarihi { get; private set; }
    public int Dakika { get; private set; }
    public HakKaynagi Kaynak { get; private set; }
    public string Aciklama { get; private set; } = string.Empty;

    public static IzinHakki Elle(int calisanId, DateOnly kazanimTarihi, int dakika, string aciklama)
    {
        if (dakika == 0)
        {
            throw new BusinessRuleException("Hak düzeltmesinin süresi sıfır olamaz.");
        }

        return new IzinHakki(calisanId, null, kazanimTarihi, dakika, HakKaynagi.Elle, aciklama);
    }

    public void ElleOldugunuDogrula()
    {
        if (Kaynak != HakKaynagi.Elle)
        {
            throw new BusinessRuleException("Otomatik hak satırı değiştirilemez. Düzeltmek için yeni bir hak satırı ekleyin.");
        }
    }

    public void Duzelt(DateOnly kazanimTarihi, int dakika, string aciklama)
    {
        ElleOldugunuDogrula();
        if (dakika == 0)
        {
            throw new BusinessRuleException("Hak düzeltmesinin süresi sıfır olamaz.");
        }

        KazanimTarihi = kazanimTarihi;
        Dakika = dakika;
        Aciklama = aciklama;
    }

    public static IzinHakki Kural(int calisanId, int kidemYili, DateOnly kazanimTarihi, int dakika, string aciklama) =>
        new(calisanId, kidemYili, kazanimTarihi, dakika, HakKaynagi.Kural, aciklama);
}
