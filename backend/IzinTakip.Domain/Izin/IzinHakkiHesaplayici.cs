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

    public static int TamYil(DateOnly baslangic, DateOnly tarih)
    {
        var yil = tarih.Year - baslangic.Year;
        return tarih < baslangic.AddYears(yil) ? yil - 1 : yil;
    }

    public static int IlkHakKidemYili(DateOnly iseGirisTarihi, DateOnly tarih) => Math.Max(1, TamYil(iseGirisTarihi, tarih) + 1);

    public static IReadOnlyList<IzinHakki> EksikHaklar(
        Calisan calisan,
        DateOnly bugun,
        IReadOnlySet<int> yazilmisKidemYillari,
        IReadOnlyCollection<IzinHakkiKurali> kurallar,
        int gunlukDakika)
    {
        var haklar = new List<IzinHakki>();
        for (var kidem = calisan.HakBaslangicKidemYili; calisan.IseGirisTarihi.AddYears(kidem) <= bugun; kidem++)
        {
            if (yazilmisKidemYillari.Contains(kidem))
            {
                continue;
            }

            var yildonumu = calisan.IseGirisTarihi.AddYears(kidem);
            if (KuralSec(kurallar, kidem, TamYil(calisan.DogumTarihi, yildonumu)) is { Gun: > 0 } kural)
            {
                haklar.Add(IzinHakki.Kural(calisan.Id, kidem, yildonumu, kural.Gun * gunlukDakika, $"{kidem}. yıl hakkı ({kural.Ad})"));
            }
        }

        return haklar;
    }
}
