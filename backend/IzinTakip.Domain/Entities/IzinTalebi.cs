using IzinTakip.Domain.Enums;
using IzinTakip.Domain.Exceptions;
using IzinTakip.Domain.Izin;

namespace IzinTakip.Domain.Entities;

public sealed class IzinTalebi : Entity
{
    private IzinTalebi()
    {
    }

    public int CalisanId { get; private set; }
    public int IzinTuruId { get; private set; }
    public SureTipi SureTipi { get; private set; }
    public DateTime Baslangic { get; private set; }
    public DateTime Bitis { get; private set; }
    public int Dakika { get; private set; }
    public IzinDurumu Durum { get; private set; }
    public string? Aciklama { get; private set; }
    public int? KararVerenKullaniciId { get; private set; }
    public DateTime? KararTarihi { get; private set; }
    public string? KararNotu { get; private set; }
    public byte[] RowVersion { get; private set; } = [];
    public Calisan Calisan { get; private set; } = null!;
    public IzinTuru IzinTuru { get; private set; } = null!;

    public ZamanAraligi Aralik => new(Baslangic, Bitis);

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
            throw new IsKuraliException("Seçilen izin türü kullanımda değil.");
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
            throw new IsKuraliException($"{tur.Ad} türünde izin {BicimAdi(sure.Tip)} alınamaz.");
        }

        var hesap = IzinSuresiHesaplayici.Hesapla(sure, tur.GunHesabi, takvim);
        var gunluk = takvim.GunlukDakika;

        if (tur.TalepBasinaEnFazlaDakika is { } talepSiniri && hesap.Dakika > talepSiniri)
        {
            throw new IsKuraliException(
                $"{tur.Ad} türünde bir talepte en çok {SureMetni.Olustur(talepSiniri, gunluk)} izin alınabilir; talep {SureMetni.Olustur(hesap.Dakika, gunluk)}.");
        }

        if (tur.YillikEnFazlaDakika is { } yillikSinir && turunYilIcindeKullanimi + hesap.Dakika > yillikSinir)
        {
            throw new IsKuraliException(
                $"{tur.Ad} türünde yılda en çok {SureMetni.Olustur(yillikSinir, gunluk)} izin alınabilir; bu yıl kullanılan {SureMetni.Olustur(turunYilIcindeKullanimi, gunluk)}, talep {SureMetni.Olustur(hesap.Dakika, gunluk)}.");
        }

        if (acikTalepler.Any(a => a.CakisirMi(hesap.Aralik)))
        {
            throw new IsKuraliException("Bu tarihlerle çakışan, beklemede ya da onaylanmış başka bir izin talebi var.");
        }

        if (tur.BakiyedenDuser && hesap.Dakika > bakiye.KullanilabilirDakika)
        {
            throw new IsKuraliException(
                $"Bakiye yetersiz: kullanılabilir {SureMetni.Olustur(bakiye.KullanilabilirDakika, gunluk)}, talep {SureMetni.Olustur(hesap.Dakika, gunluk)}.");
        }

        if (hesap.Aralik.Baslangic < simdi && !gecmisTarihYetkisi)
        {
            throw new IsKuraliException("Geçmiş tarihli izin talebi açılamaz.");
        }

        return new IzinTalebi
        {
            CalisanId = calisanId,
            IzinTuruId = tur.Id,
            SureTipi = sure.Tip,
            Baslangic = hesap.Aralik.Baslangic,
            Bitis = hesap.Aralik.Bitis,
            Dakika = hesap.Dakika,
            Durum = IzinDurumu.Beklemede,
            Aciklama = aciklama
        };
    }

    private static string BicimAdi(SureTipi tip) => tip switch
    {
        SureTipi.TamGun => "tam gün olarak",
        SureTipi.YarimGun => "yarım gün olarak",
        _ => "saatlik olarak"
    };
}
