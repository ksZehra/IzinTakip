using IzinTakip.Domain.Enums;
using IzinTakip.Domain.Exceptions;

namespace IzinTakip.Domain.Izin;

public static class IzinSuresiCalculator
{
    public static IzinSuresi Hesapla(SureIstegi istek, GunHesabi gunHesabi, CalismaTakvimi takvim) => istek.Tip switch
    {
        SureTipi.TamGun => TamGun(istek, gunHesabi, takvim),
        SureTipi.YarimGun => YarimGun(istek, takvim),
        SureTipi.Saatlik => Saatlik(istek, takvim),
        _ => throw new BusinessRuleException("Süre biçimi geçersiz.")
    };

    private static IzinSuresi TamGun(SureIstegi istek, GunHesabi gunHesabi, CalismaTakvimi takvim)
    {
        var bitisTarihi = istek.BitisTarihi ?? istek.Tarih;
        if (bitisTarihi < istek.Tarih)
        {
            throw new BusinessRuleException("Bitiş tarihi başlangıç tarihinden önce olamaz.");
        }

        var toplam = 0;
        for (var gun = istek.Tarih; gun <= bitisTarihi; gun = gun.AddDays(1))
        {
            if (gunHesabi == GunHesabi.TakvimGunu)
            {
                toplam += takvim.GunlukDakika;
            }
            else if (takvim.CalisilanGunMu(gun))
            {
                toplam += takvim.ArifeMi(gun) ? takvim.GunlukDakika / 2 : takvim.GunlukDakika;
            }
        }

        if (toplam == 0)
        {
            throw new BusinessRuleException("Seçilen aralıkta çalışma günü yok.");
        }

        return new IzinSuresi(
            toplam,
            new ZamanAraligi(istek.Tarih.ToDateTime(takvim.MesaiBaslangic), bitisTarihi.ToDateTime(takvim.MesaiBitis)));
    }

    private static IzinSuresi YarimGun(SureIstegi istek, CalismaTakvimi takvim)
    {
        CalisilanGunOlmali(istek.Tarih, takvim);
        var yari = istek.GunYarisi ?? throw new BusinessRuleException("Yarım gün için sabah ya da öğleden sonra seçin.");
        if (yari == GunYarisi.OgledenSonra && takvim.ArifeMi(istek.Tarih))
        {
            throw new BusinessRuleException("Bu gün öğleden sonra zaten resmi tatil.");
        }

        var aralik = yari == GunYarisi.Sabah
            ? new ZamanAraligi(istek.Tarih.ToDateTime(takvim.MesaiBaslangic), istek.Tarih.ToDateTime(takvim.OgleArasiBaslangic))
            : new ZamanAraligi(istek.Tarih.ToDateTime(takvim.OgleArasiBitis), istek.Tarih.ToDateTime(takvim.MesaiBitis));
        return new IzinSuresi(takvim.GunlukDakika / 2, aralik);
    }

    private static IzinSuresi Saatlik(SureIstegi istek, CalismaTakvimi takvim)
    {
        CalisilanGunOlmali(istek.Tarih, takvim);
        if (istek.BaslangicSaati is not { } baslangic || istek.BitisSaati is not { } bitis)
        {
            throw new BusinessRuleException("Saatlik izin için başlangıç ve bitiş saati seçin.");
        }

        if (baslangic >= bitis || baslangic < takvim.MesaiBaslangic || bitis > takvim.MesaiBitis)
        {
            throw new BusinessRuleException($"Saatlik izin mesai saatleri ({takvim.MesaiBaslangic:HH\\:mm}–{takvim.MesaiBitis:HH\\:mm}) içinde olmalı ve başlangıç bitişten önce gelmeli.");
        }

        if (GunIciDakika(baslangic) % takvim.SaatlikIzinAdimDakika != 0 || GunIciDakika(bitis) % takvim.SaatlikIzinAdimDakika != 0)
        {
            throw new BusinessRuleException($"Saatlik izin {takvim.SaatlikIzinAdimDakika} dakikalık adımlarla seçilir.");
        }

        if (takvim.ArifeMi(istek.Tarih) && bitis > takvim.OgleArasiBaslangic)
        {
            throw new BusinessRuleException("Bu gün öğleden sonra resmi tatil; saatlik izin öğle arasından önce bitmeli.");
        }

        var aralik = new ZamanAraligi(istek.Tarih.ToDateTime(baslangic), istek.Tarih.ToDateTime(bitis));
        var ogleArasi = new ZamanAraligi(istek.Tarih.ToDateTime(takvim.OgleArasiBaslangic), istek.Tarih.ToDateTime(takvim.OgleArasiBitis));
        var dakika = aralik.Dakika - aralik.KesisimDakika(ogleArasi);
        if (dakika <= 0)
        {
            throw new BusinessRuleException("Seçilen saatler öğle arasına denk geliyor; çalışma saati seçin.");
        }

        return new IzinSuresi(dakika, aralik);
    }

    private static void CalisilanGunOlmali(DateOnly tarih, CalismaTakvimi takvim)
    {
        if (!takvim.CalisilanGunMu(tarih))
        {
            throw new BusinessRuleException("Seçilen gün çalışma günü değil.");
        }
    }

    private static int GunIciDakika(TimeOnly saat) => saat.Hour * 60 + saat.Minute;
}
