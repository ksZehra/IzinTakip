using IzinTakip.Domain.Enums;
using IzinTakip.Domain.Exceptions;

namespace IzinTakip.Domain.Entities;

public sealed class IzinHakki : Entity
{
    private IzinHakki()
    {
    }

    private IzinHakki(int calisanId, DateOnly kazanimTarihi, int dakika, HakKaynagi kaynak, string aciklama)
    {
        CalisanId = calisanId;
        KazanimTarihi = kazanimTarihi;
        Dakika = dakika;
        Kaynak = kaynak;
        Aciklama = aciklama;
    }

    public int CalisanId { get; private set; }
    public DateOnly KazanimTarihi { get; private set; }
    public int Dakika { get; private set; }
    public HakKaynagi Kaynak { get; private set; }
    public string Aciklama { get; private set; } = string.Empty;

    public static IzinHakki Elle(int calisanId, DateOnly kazanimTarihi, int dakika, string aciklama)
    {
        if (dakika == 0)
        {
            throw new IsKuraliException("Hak düzeltmesinin süresi sıfır olamaz.");
        }

        return new IzinHakki(calisanId, kazanimTarihi, dakika, HakKaynagi.Elle, aciklama);
    }

    public static IzinHakki Kural(int calisanId, DateOnly kazanimTarihi, int dakika, string aciklama) =>
        new(calisanId, kazanimTarihi, dakika, HakKaynagi.Kural, aciklama);
}
