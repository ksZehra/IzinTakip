using IzinTakip.Domain.Enums;
using IzinTakip.Domain.Exceptions;

namespace IzinTakip.Domain.Entities;

public sealed class Ayarlar : Entity
{
    public const int TekSatirId = 1;

    private Ayarlar()
    {
    }

    public Ayarlar(
        HaftaGunleri calismaGunleri,
        TimeOnly mesaiBaslangic,
        TimeOnly mesaiBitis,
        TimeOnly ogleArasiBaslangic,
        TimeOnly ogleArasiBitis,
        int saatlikIzinAdimDakika,
        int hatirlatmaGunSayisi) =>
        Guncelle(calismaGunleri, mesaiBaslangic, mesaiBitis, ogleArasiBaslangic, ogleArasiBitis, saatlikIzinAdimDakika, hatirlatmaGunSayisi);

    public HaftaGunleri CalismaGunleri { get; private set; }
    public TimeOnly MesaiBaslangic { get; private set; }
    public TimeOnly MesaiBitis { get; private set; }
    public TimeOnly OgleArasiBaslangic { get; private set; }
    public TimeOnly OgleArasiBitis { get; private set; }
    public int SaatlikIzinAdimDakika { get; private set; }
    public int HatirlatmaGunSayisi { get; private set; }

    public int GunlukCalismaDakikasi => GunlukDakika(MesaiBaslangic, MesaiBitis, OgleArasiBaslangic, OgleArasiBitis);

    public void Guncelle(
        HaftaGunleri calismaGunleri,
        TimeOnly mesaiBaslangic,
        TimeOnly mesaiBitis,
        TimeOnly ogleArasiBaslangic,
        TimeOnly ogleArasiBitis,
        int saatlikIzinAdimDakika,
        int hatirlatmaGunSayisi)
    {
        if (calismaGunleri == HaftaGunleri.Yok)
        {
            throw new BusinessRuleException("En az bir çalışma günü seçilmeli.");
        }

        if (!(mesaiBaslangic < mesaiBitis && mesaiBaslangic <= ogleArasiBaslangic && ogleArasiBaslangic <= ogleArasiBitis && ogleArasiBitis <= mesaiBitis))
        {
            throw new BusinessRuleException("Mesai başlangıcı bitişten önce olmalı; öğle arası mesai saatlerinin içinde kalmalı.");
        }

        var gunluk = GunlukDakika(mesaiBaslangic, mesaiBitis, ogleArasiBaslangic, ogleArasiBitis);
        if (gunluk <= 0 || gunluk % 2 != 0)
        {
            throw new BusinessRuleException("Günlük çalışma süresi sıfırdan büyük olmalı ve yarım güne tam bölünebilmeli (çift dakika).");
        }

        CalismaGunleri = calismaGunleri;
        MesaiBaslangic = mesaiBaslangic;
        MesaiBitis = mesaiBitis;
        OgleArasiBaslangic = ogleArasiBaslangic;
        OgleArasiBitis = ogleArasiBitis;
        SaatlikIzinAdimDakika = saatlikIzinAdimDakika;
        HatirlatmaGunSayisi = hatirlatmaGunSayisi;
    }

    public static int GunlukDakika(TimeOnly mesaiBaslangic, TimeOnly mesaiBitis, TimeOnly ogleArasiBaslangic, TimeOnly ogleArasiBitis) =>
        (int)(mesaiBitis - mesaiBaslangic).TotalMinutes - (int)(ogleArasiBitis - ogleArasiBaslangic).TotalMinutes;
}
