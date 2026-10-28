using IzinTakip.Domain.Enums;
using IzinTakip.Domain.Exceptions;

namespace IzinTakip.Domain.Entities;

public sealed class IzinTuru : Entity
{
    private IzinTuru()
    {
    }

    public IzinTuru(
        string ad,
        bool bakiyedenDuser,
        bool belgeZorunlu,
        GunHesabi gunHesabi,
        bool tamGunAlinir,
        bool yarimGunAlinir,
        bool saatlikAlinir,
        int? talepBasinaEnFazlaDakika,
        int? yillikEnFazlaDakika)
    {
        Aktif = true;
        Guncelle(ad, bakiyedenDuser, belgeZorunlu, gunHesabi, tamGunAlinir, yarimGunAlinir, saatlikAlinir, talepBasinaEnFazlaDakika, yillikEnFazlaDakika);
    }

    public string Ad { get; private set; } = string.Empty;
    public bool BakiyedenDuser { get; private set; }
    public bool BelgeZorunlu { get; private set; }
    public GunHesabi GunHesabi { get; private set; }
    public bool TamGunAlinir { get; private set; }
    public bool YarimGunAlinir { get; private set; }
    public bool SaatlikAlinir { get; private set; }
    public int? TalepBasinaEnFazlaDakika { get; private set; }
    public int? YillikEnFazlaDakika { get; private set; }
    public bool Aktif { get; private set; }

    public void Guncelle(
        string ad,
        bool bakiyedenDuser,
        bool belgeZorunlu,
        GunHesabi gunHesabi,
        bool tamGunAlinir,
        bool yarimGunAlinir,
        bool saatlikAlinir,
        int? talepBasinaEnFazlaDakika,
        int? yillikEnFazlaDakika)
    {
        if (!tamGunAlinir && !yarimGunAlinir && !saatlikAlinir)
        {
            throw new IsKuraliException("En az bir süre biçimi seçilmeli: tam gün, yarım gün ya da saatlik.");
        }

        if (gunHesabi == GunHesabi.TakvimGunu && (yarimGunAlinir || saatlikAlinir))
        {
            throw new IsKuraliException("Takvim günü sayan izin türü yalnız tam gün alınabilir.");
        }

        if (talepBasinaEnFazlaDakika is not null && yillikEnFazlaDakika is not null && talepBasinaEnFazlaDakika > yillikEnFazlaDakika)
        {
            throw new IsKuraliException("Talep başına sınır, yıllık sınırdan büyük olamaz.");
        }

        Ad = ad;
        BakiyedenDuser = bakiyedenDuser;
        BelgeZorunlu = belgeZorunlu;
        GunHesabi = gunHesabi;
        TamGunAlinir = tamGunAlinir;
        YarimGunAlinir = yarimGunAlinir;
        SaatlikAlinir = saatlikAlinir;
        TalepBasinaEnFazlaDakika = talepBasinaEnFazlaDakika;
        YillikEnFazlaDakika = yillikEnFazlaDakika;
    }

    public void AktiflikDegistir(bool aktif) => Aktif = aktif;
}
