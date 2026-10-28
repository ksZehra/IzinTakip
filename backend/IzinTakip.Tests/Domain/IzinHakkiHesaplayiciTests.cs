using IzinTakip.Domain.Entities;
using IzinTakip.Domain.Enums;
using IzinTakip.Domain.Izin;

namespace IzinTakip.Tests.Domain;

public sealed class IzinHakkiHesaplayiciTests
{
    private const int Gunluk = 480;

    private static readonly IzinHakkiKurali[] Kurallar =
    [
        new("1–5 yıl", 1, 5, null, null, 14),
        new("6–14 yıl", 6, 14, null, null, 20),
        new("15 yıl ve üstü", 15, null, null, null, 26),
        new("18 yaş ve altı", 1, null, null, 18, 20),
        new("50 yaş ve üstü", 1, null, 50, null, 20)
    ];

    [Theory]
    [InlineData(1, 30, 14)]
    [InlineData(5, 30, 14)]
    [InlineData(6, 30, 20)]
    [InlineData(14, 40, 20)]
    [InlineData(15, 40, 26)]
    [InlineData(3, 17, 20)]
    [InlineData(3, 50, 20)]
    [InlineData(20, 55, 26)]
    public void KuralSec_KidemVeYas_EnYuksekGunluKuraliSecer(int kidem, int yas, int beklenenGun)
    {
        Assert.Equal(beklenenGun, IzinHakkiHesaplayici.KuralSec(Kurallar, kidem, yas)?.Gun);
    }

    [Fact]
    public void KuralSec_HicbirKuralTutmaz_BosDoner()
    {
        Assert.Null(IzinHakkiHesaplayici.KuralSec(Kurallar, 0, 30));
    }

    [Fact]
    public void KuralSec_PasifKural_HesabaKatilmaz()
    {
        var pasif = new IzinHakkiKurali("Eski kural", 1, null, null, null, 30);
        pasif.AktiflikDegistir(false);

        Assert.Equal(14, IzinHakkiHesaplayici.KuralSec([.. Kurallar, pasif], 2, 30)?.Gun);
    }

    [Theory]
    [InlineData("2027-05-14", 36)]
    [InlineData("2027-05-15", 37)]
    public void Yas_DogumGunundenOnceVeSonra_DogruHesaplanir(string tarih, int beklenen)
    {
        Assert.Equal(beklenen, IzinHakkiHesaplayici.Yas(new DateOnly(1990, 5, 15), DateOnly.Parse(tarih)));
    }

    [Fact]
    public void EksikHaklar_KayittanSonrakiYildonumleri_KuralGunuIleYazilir()
    {
        var calisan = new Calisan("Ayşe", "Yılmaz", 1, new DateOnly(2020, 3, 10), new DateOnly(1990, 5, 15));

        var haklar = IzinHakkiHesaplayici.EksikHaklar(calisan, new DateOnly(2024, 1, 1), new DateOnly(2026, 3, 10), new HashSet<DateOnly>(), Kurallar, Gunluk);

        Assert.Equal([new DateOnly(2024, 3, 10), new DateOnly(2025, 3, 10), new DateOnly(2026, 3, 10)], haklar.Select(h => h.KazanimTarihi));
        Assert.Equal([14 * Gunluk, 14 * Gunluk, 20 * Gunluk], haklar.Select(h => h.Dakika));
        Assert.All(haklar, h => Assert.Equal(HakKaynagi.Kural, h.Kaynak));
        Assert.Equal("6. yıl hakkı (6–14 yıl)", haklar[^1].Aciklama);
    }

    [Fact]
    public void EksikHaklar_YazilmisVeKayitGunundekiYildonumu_Atlanir()
    {
        var calisan = new Calisan("Can", "Öz", 1, new DateOnly(2020, 3, 10), new DateOnly(1990, 5, 15));

        var haklar = IzinHakkiHesaplayici.EksikHaklar(
            calisan, new DateOnly(2024, 3, 10), new DateOnly(2026, 3, 9), new HashSet<DateOnly> { new(2025, 3, 10) }, Kurallar, Gunluk);

        Assert.Empty(haklar);
    }
}
