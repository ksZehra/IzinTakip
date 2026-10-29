using IzinTakip.Domain.Enums;
using IzinTakip.Domain.Exceptions;
using IzinTakip.Domain.Izin;

namespace IzinTakip.Domain.Entities;

public sealed class IzinTalebi : Entity
{
    public const int EnFazlaBelge = 5;

    private IzinTalebi()
    {
    }

    public int CalisanId { get; private set; }
    public int IzinTuruId { get; private set; }
    public SureTipi SureTipi { get; private set; }
    public DateTime Baslangic { get; private set; }
    public DateTime Bitis { get; private set; }
    public int Dakika { get; private set; }
    public bool BakiyedenDuser { get; private set; }
    public IzinDurumu Durum { get; private set; }
    public string? Aciklama { get; private set; }
    public int? KararVerenKullaniciId { get; private set; }
    public DateTime? KararTarihi { get; private set; }
    public string? KararNotu { get; private set; }
    public DateOnly? SonHatirlatmaTarihi { get; private set; }
    public byte[] RowVersion { get; private set; } = [];
    public Calisan Calisan { get; private set; } = null!;
    public IzinTuru IzinTuru { get; private set; } = null!;

    public static IzinTalebi Olustur(
        int calisanId,
        IzinTuru tur,
        SureIstegi sure,
        string? aciklama,
        CalismaTakvimi takvim,
        IReadOnlyCollection<ZamanAraligi> acikTalepler,
        Bakiye bakiye,
        int turunYilIcindeKullanimi,
        DateTime simdi,
        bool gecmisTarihYetkisi)
    {
        if (!tur.Aktif)
        {
            throw new BusinessRuleException("Seçilen izin türü kullanımda değil.");
        }

        var bicimAcik = sure.Tip switch
        {
            SureTipi.TamGun => tur.TamGunAlinir,
            SureTipi.YarimGun => tur.YarimGunAlinir,
            SureTipi.Saatlik => tur.SaatlikAlinir,
            _ => false
        };
        if (!bicimAcik)
        {
            throw new BusinessRuleException($"{tur.Ad} türünde izin {BicimAdi(sure.Tip)} alınamaz.");
        }

        var hesap = IzinSuresiCalculator.Hesapla(sure, tur.GunHesabi, takvim);
        var gunluk = takvim.GunlukDakika;

        if (tur.TalepBasinaEnFazlaDakika is { } talepSiniri && hesap.Dakika > talepSiniri)
        {
            throw new BusinessRuleException(
                $"{tur.Ad} türünde bir talepte en çok {SureMetni.Olustur(talepSiniri, gunluk)} izin alınabilir; talep {SureMetni.Olustur(hesap.Dakika, gunluk)}.");
        }

        if (tur.YillikEnFazlaDakika is { } yillikSinir)
        {
            if (hesap.Aralik.Baslangic.Year != hesap.Aralik.Bitis.Year)
            {
                throw new BusinessRuleException(
                    $"{tur.Ad} türünde yıllık sınır olduğu için talep yıl sonunu aşamaz. Talebi 31 Aralık'a kadar ve yeni yıl için iki ayrı talep olarak açın.");
            }

            if (turunYilIcindeKullanimi + hesap.Dakika > yillikSinir)
            {
                throw new BusinessRuleException(
                    $"{tur.Ad} türünde yılda en çok {SureMetni.Olustur(yillikSinir, gunluk)} izin alınabilir; bu yıl kullanılan {SureMetni.Olustur(turunYilIcindeKullanimi, gunluk)}, talep {SureMetni.Olustur(hesap.Dakika, gunluk)}.");
            }
        }

        var cakismaAraligi = CakismaAraligi(sure.Tip, hesap.Aralik);
        if (acikTalepler.Any(a => a.CakisirMi(cakismaAraligi)))
        {
            throw new BusinessRuleException("Bu tarihlerle çakışan, beklemede ya da onaylanmış başka bir izin talebi var.");
        }

        if (tur.BakiyedenDuser && hesap.Dakika > bakiye.KullanilabilirDakika)
        {
            throw new BusinessRuleException(
                $"Bakiye yetersiz: kullanılabilir {SureMetni.Olustur(bakiye.KullanilabilirDakika, gunluk)}, talep {SureMetni.Olustur(hesap.Dakika, gunluk)}.");
        }

        if (hesap.Aralik.Baslangic < simdi && !gecmisTarihYetkisi)
        {
            throw new BusinessRuleException("Geçmiş tarihli izin talebi açılamaz.");
        }

        return new IzinTalebi
        {
            CalisanId = calisanId,
            IzinTuruId = tur.Id,
            SureTipi = sure.Tip,
            Baslangic = hesap.Aralik.Baslangic,
            Bitis = hesap.Aralik.Bitis,
            Dakika = hesap.Dakika,
            BakiyedenDuser = tur.BakiyedenDuser,
            Durum = IzinDurumu.Beklemede,
            Aciklama = aciklama
        };
    }

    public static ZamanAraligi CakismaAraligi(SureTipi tip, ZamanAraligi aralik) =>
        tip == SureTipi.TamGun ? new ZamanAraligi(aralik.Baslangic.Date, aralik.Bitis.Date.AddDays(1)) : aralik;

    public void Onayla(
        int kararVerenKullaniciId,
        int? kararVerenCalisanId,
        bool superAdmin,
        string? not,
        DateTime kararZamani,
        bool belgeZorunlu,
        int belgeSayisi)
    {
        KararVerilebilirMi(kararVerenCalisanId, superAdmin, "onaylayamazsınız");
        if (belgeZorunlu && belgeSayisi == 0)
        {
            throw new BusinessRuleException("Bu izin türü belge ister; talebe belge eklenmeden onaylanamaz.");
        }

        Durum = IzinDurumu.Onaylandi;
        KarariYaz(kararVerenKullaniciId, not, kararZamani);
    }

    public void Reddet(int kararVerenKullaniciId, int? kararVerenCalisanId, bool superAdmin, string gerekce, DateTime kararZamani)
    {
        if (string.IsNullOrWhiteSpace(gerekce))
        {
            throw new BusinessRuleException("Red gerekçesi zorunludur.");
        }

        KararVerilebilirMi(kararVerenCalisanId, superAdmin, "reddedemezsiniz");
        Durum = IzinDurumu.Reddedildi;
        KarariYaz(kararVerenKullaniciId, gerekce, kararZamani);
    }

    public void IptalEt(int iptalEdenKullaniciId, string? not, DateTime simdi, DateTime kararZamani, bool baslamisIzniIptalYetkisi)
    {
        if (Durum is not (IzinDurumu.Beklemede or IzinDurumu.Onaylandi))
        {
            throw new BusinessRuleException("Yalnız beklemedeki ya da onaylanmış talep iptal edilebilir.");
        }

        if (Baslangic <= simdi && !baslamisIzniIptalYetkisi)
        {
            throw new BusinessRuleException("Başlamış ya da geçmiş izin iptal edilemez.");
        }

        Durum = IzinDurumu.IptalEdildi;
        KarariYaz(iptalEdenKullaniciId, not, kararZamani);
    }

    public void HatirlatmaYapildi(DateOnly tarih) => SonHatirlatmaTarihi = tarih;

    public void BelgeEklemeyiDenetle(int mevcutBelgeSayisi)
    {
        if (Durum is not (IzinDurumu.Beklemede or IzinDurumu.Onaylandi))
        {
            throw new BusinessRuleException("Reddedilmiş ya da iptal edilmiş talebe belge eklenemez.");
        }

        if (mevcutBelgeSayisi >= EnFazlaBelge)
        {
            throw new BusinessRuleException($"Bir talebe en çok {EnFazlaBelge} belge eklenebilir.");
        }
    }

    public void BelgeSilmeyiDenetle()
    {
        if (Durum != IzinDurumu.Beklemede)
        {
            throw new BusinessRuleException("Belge yalnız beklemedeki talepten silinebilir.");
        }
    }

    private void KararVerilebilirMi(int? kararVerenCalisanId, bool superAdmin, string eylem)
    {
        if (Durum != IzinDurumu.Beklemede)
        {
            throw new BusinessRuleException("Bu talep artık beklemede değil.");
        }

        if (kararVerenCalisanId == CalisanId && !superAdmin)
        {
            throw new BusinessRuleException($"Kendi izin talebinizi {eylem}.");
        }
    }

    private void KarariYaz(int kullaniciId, string? not, DateTime kararZamani)
    {
        KararVerenKullaniciId = kullaniciId;
        KararNotu = string.IsNullOrWhiteSpace(not) ? null : not.Trim();
        KararTarihi = kararZamani;
    }

    private static string BicimAdi(SureTipi tip) => tip switch
    {
        SureTipi.TamGun => "tam gün olarak",
        SureTipi.YarimGun => "yarım gün olarak",
        _ => "saatlik olarak"
    };
}
