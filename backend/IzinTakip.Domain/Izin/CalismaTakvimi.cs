using IzinTakip.Domain.Entities;
using IzinTakip.Domain.Enums;

namespace IzinTakip.Domain.Izin;

public sealed class CalismaTakvimi
{
    private readonly Dictionary<DateOnly, bool> _resmiTatiller;

    public CalismaTakvimi(
        HaftaGunleri calismaGunleri,
        TimeOnly mesaiBaslangic,
        TimeOnly mesaiBitis,
        TimeOnly ogleArasiBaslangic,
        TimeOnly ogleArasiBitis,
        int saatlikIzinAdimDakika,
        IEnumerable<(DateOnly Tarih, bool YarimGun)> resmiTatiller)
    {
        CalismaGunleri = calismaGunleri;
        MesaiBaslangic = mesaiBaslangic;
        MesaiBitis = mesaiBitis;
        OgleArasiBaslangic = ogleArasiBaslangic;
        OgleArasiBitis = ogleArasiBitis;
        SaatlikIzinAdimDakika = saatlikIzinAdimDakika;
        GunlukDakika = Ayarlar.GunlukDakika(mesaiBaslangic, mesaiBitis, ogleArasiBaslangic, ogleArasiBitis);
        _resmiTatiller = resmiTatiller.ToDictionary(t => t.Tarih, t => t.YarimGun);
    }

    public HaftaGunleri CalismaGunleri { get; }
    public TimeOnly MesaiBaslangic { get; }
    public TimeOnly MesaiBitis { get; }
    public TimeOnly OgleArasiBaslangic { get; }
    public TimeOnly OgleArasiBitis { get; }
    public int SaatlikIzinAdimDakika { get; }
    public int GunlukDakika { get; }

    public static CalismaTakvimi Olustur(Ayarlar ayarlar, IEnumerable<ResmiTatil> resmiTatiller) => new(
        ayarlar.CalismaGunleri,
        ayarlar.MesaiBaslangic,
        ayarlar.MesaiBitis,
        ayarlar.OgleArasiBaslangic,
        ayarlar.OgleArasiBitis,
        ayarlar.SaatlikIzinAdimDakika,
        resmiTatiller.Select(t => (t.Tarih, t.YarimGun)));

    public bool CalisilanGunMu(DateOnly tarih) =>
        CalismaGunleri.HasFlag(HaftaGunu(tarih.DayOfWeek)) && !(_resmiTatiller.TryGetValue(tarih, out var yarimGun) && !yarimGun);

    public bool ArifeMi(DateOnly tarih) => _resmiTatiller.TryGetValue(tarih, out var yarimGun) && yarimGun;

    private static HaftaGunleri HaftaGunu(DayOfWeek gun) => gun switch
    {
        DayOfWeek.Monday => HaftaGunleri.Pazartesi,
        DayOfWeek.Tuesday => HaftaGunleri.Sali,
        DayOfWeek.Wednesday => HaftaGunleri.Carsamba,
        DayOfWeek.Thursday => HaftaGunleri.Persembe,
        DayOfWeek.Friday => HaftaGunleri.Cuma,
        DayOfWeek.Saturday => HaftaGunleri.Cumartesi,
        _ => HaftaGunleri.Pazar
    };
}
