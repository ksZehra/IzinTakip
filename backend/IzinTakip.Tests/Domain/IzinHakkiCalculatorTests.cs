using IzinTakip.Domain.Entities;
using IzinTakip.Domain.Enums;
using IzinTakip.Domain.Izin;

namespace IzinTakip.Tests.Domain;

public sealed class IzinHakkiCalculatorTests
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
        Assert.Equal(beklenenGun, IzinHakkiCalculator.KuralSec(Kurallar, kidem, yas)?.Gun);
    }

    [Fact]
    public void KuralSec_HicbirKuralTutmaz_BosDoner()
    {
        Assert.Null(IzinHakkiCalculator.KuralSec(Kurallar, 0, 30));
    }

    [Fact]
    public void KuralSec_PasifKural_HesabaKatilmaz()
    {
        var pasif = new IzinHakkiKurali("Eski kural", 1, null, null, null, 30);
        pasif.AktiflikDegistir(false);

        Assert.Equal(14, IzinHakkiCalculator.KuralSec([.. Kurallar, pasif], 2, 30)?.Gun);
    }

    [Theory]
    [InlineData("2027-05-14", 36)]
    [InlineData("2027-05-15", 37)]
    public void TamYil_YildonumundenOnceVeSonra_DogruHesaplanir(string tarih, int beklenen)
    {
        Assert.Equal(beklenen, IzinHakkiCalculator.TamYil(new DateOnly(1990, 5, 15), DateOnly.Parse(tarih)));
    }

    [Theory]
    [InlineData("2020-03-10", "2024-01-01", 4)]
    [InlineData("2020-03-10", "2024-03-10", 5)]
    [InlineData("2026-05-01", "2026-04-01", 1)]
    public void IlkHakKidemYili_KayitTarihindenSonrakiIlkYil(string iseGiris, string kayit, int beklenen)
    {
        Assert.Equal(beklenen, IzinHakkiCalculator.IlkHakKidemYili(DateOnly.Parse(iseGiris), DateOnly.Parse(kayit)));
    }

    [Fact]
    public void EksikHaklar_BaslangicYilindanItibaren_KuralGunuIleYazilir()
    {
        var calisan = new Calisan("Ayşe", "Yılmaz", 1, new DateOnly(2020, 3, 10), new DateOnly(1990, 5, 15), 4);

        var haklar = IzinHakkiCalculator.EksikHaklar(calisan, new DateOnly(2026, 3, 10), new HashSet<int>(), Kurallar, Gunluk);

        Assert.Equal([4, 5, 6], haklar.Select(h => h.KidemYili));
        Assert.Equal([new DateOnly(2024, 3, 10), new DateOnly(2025, 3, 10), new DateOnly(2026, 3, 10)], haklar.Select(h => h.KazanimTarihi));
        Assert.Equal([14 * Gunluk, 14 * Gunluk, 20 * Gunluk], haklar.Select(h => h.Dakika));
        Assert.All(haklar, h => Assert.Equal(HakKaynagi.Kural, h.Kaynak));
        Assert.Equal("6. yıl hakkı (6–14 yıl)", haklar[^1].Aciklama);
    }

    [Fact]
    public void EksikHaklar_YazilmisKidemYili_TekrarYazilmaz()
    {
        var calisan = new Calisan("Can", "Öz", 1, new DateOnly(2020, 3, 10), new DateOnly(1990, 5, 15), 4);

        var haklar = IzinHakkiCalculator.EksikHaklar(calisan, new DateOnly(2026, 3, 9), new HashSet<int> { 4, 5 }, Kurallar, Gunluk);

        Assert.Empty(haklar);
    }

    [Theory]
    [InlineData("2020-04-15", new int[0])]
    [InlineData("2019-09-15", new[] { 7 })]
    public void EksikHaklar_IseGirisTarihiOneCekilince_YalnizYeniDolanYilYazilir(string yeniIseGiris, int[] beklenenYillar)
    {
        var calisan = new Calisan("Ali", "Ak", 1, DateOnly.Parse(yeniIseGiris), new DateOnly(1990, 5, 15), 4);

        var haklar = IzinHakkiCalculator.EksikHaklar(calisan, new DateOnly(2026, 10, 29), new HashSet<int> { 4, 5, 6 }, Kurallar, Gunluk);

        Assert.Equal(beklenenYillar, haklar.Select(h => h.KidemYili!.Value));
    }
}
