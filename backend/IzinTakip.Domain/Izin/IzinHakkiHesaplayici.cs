using IzinTakip.Domain.Entities;

namespace IzinTakip.Domain.Izin;

public static class IzinHakkiHesaplayici
{
    public static IzinHakkiKurali? KuralSec(IEnumerable<IzinHakkiKurali> kurallar, int kidemYil, int yas) =>
        kurallar
            .Where(k => k.Aktif
                        && kidemYil >= k.MinKidemYil
                        && (k.MaxKidemYil is null || kidemYil <= k.MaxKidemYil)
                        && (k.MinYas is null || yas >= k.MinYas)
                        && (k.MaxYas is null || yas <= k.MaxYas))
            .MaxBy(k => k.Gun);

    public static int Yas(DateOnly dogumTarihi, DateOnly tarih)
    {
        var yas = tarih.Year - dogumTarihi.Year;
        return tarih < dogumTarihi.AddYears(yas) ? yas - 1 : yas;
    }

    public static IReadOnlyList<IzinHakki> EksikHaklar(
        Calisan calisan,
        DateOnly kayitTarihi,
        DateOnly bugun,
        IReadOnlySet<DateOnly> yazilmisYildonumleri,
        IReadOnlyCollection<IzinHakkiKurali> kurallar,
        int gunlukDakika)
    {
        var haklar = new List<IzinHakki>();
        for (var kidem = 1; calisan.IseGirisTarihi.AddYears(kidem) <= bugun; kidem++)
        {
            var yildonumu = calisan.IseGirisTarihi.AddYears(kidem);
            if (yildonumu <= kayitTarihi || yazilmisYildonumleri.Contains(yildonumu))
            {
                continue;
            }

            if (KuralSec(kurallar, kidem, Yas(calisan.DogumTarihi, yildonumu)) is { Gun: > 0 } kural)
            {
                haklar.Add(IzinHakki.Kural(calisan.Id, yildonumu, kural.Gun * gunlukDakika, $"{kidem}. yıl hakkı ({kural.Ad})"));
            }
        }

        return haklar;
    }
}
