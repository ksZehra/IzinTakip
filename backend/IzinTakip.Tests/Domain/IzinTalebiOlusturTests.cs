using IzinTakip.Domain.Entities;
using IzinTakip.Domain.Enums;
using IzinTakip.Domain.Exceptions;
using IzinTakip.Domain.Izin;
using static IzinTakip.Tests.Domain.TestTakvimi;

namespace IzinTakip.Tests.Domain;

public sealed class IzinTalebiOlusturTests
{
    private static readonly DateTime Simdi = new(2026, 2, 2, 10, 0, 0);
    private static readonly Bakiye OnDortGun = new(14 * 480, 0, 0);
    private static readonly IzinTuru Yillik = new("Yıllık", true, false, GunHesabi.IsGunu, true, true, false, null, null);

    [Fact]
    public void Olustur_GecerliTamGun_BeklemedeVeSuresiDolu()
    {
        var talep = Olustur(Yillik, TamGun(Pazartesi, Pazartesi.AddDays(2)));

        Assert.Equal(IzinDurumu.Beklemede, talep.Durum);
        Assert.Equal(1440, talep.Dakika);
        Assert.Equal(SureTipi.TamGun, talep.SureTipi);
    }

    [Fact]
    public void Olustur_BicimTurdeKapali_IsKuraliHatasiVerir()
    {
        var hata = Assert.Throws<IsKuraliException>(() =>
            Olustur(Yillik, new SureIstegi(SureTipi.Saatlik, Pazartesi, null, null, new(10, 0), new(11, 0))));

        Assert.Equal("Yıllık türünde izin saatlik olarak alınamaz.", hata.Message);
    }

    [Fact]
    public void Olustur_TalepBasinaSinirAsildi_IsKuraliHatasiVerir()
    {
        var evlilik = new IzinTuru("Evlilik", false, false, GunHesabi.IsGunu, true, false, false, 3 * 480, null);

        var hata = Assert.Throws<IsKuraliException>(() => Olustur(evlilik, TamGun(Pazartesi, Pazartesi.AddDays(3))));

        Assert.Equal("Evlilik türünde bir talepte en çok 3 gün izin alınabilir; talep 4 gün.", hata.Message);
    }

    [Fact]
    public void Olustur_YillikSinirAsildi_IsKuraliHatasiVerir()
    {
        var mazeret = new IzinTuru("Mazeret", false, false, GunHesabi.IsGunu, true, true, false, null, 2 * 480);

        Assert.Throws<IsKuraliException>(() =>
            Olustur(mazeret, TamGun(Pazartesi, Pazartesi), turunYilIcindeKullanimi: 720));
    }

    [Fact]
    public void Olustur_AcikTalepleCakisiyor_IsKuraliHatasiVerir()
    {
        var mevcut = new ZamanAraligi(Pazartesi.ToDateTime(new(9, 0)), Pazartesi.ToDateTime(new(12, 30)));

        Assert.Throws<IsKuraliException>(() => Olustur(Yillik, TamGun(Pazartesi, Pazartesi), acikTalepler: [mevcut]));
    }

    [Fact]
    public void Olustur_SabahIzinliykenOgledenSonra_Cakismaz()
    {
        var sabah = new ZamanAraligi(Pazartesi.ToDateTime(new(9, 0)), Pazartesi.ToDateTime(new(12, 30)));

        var talep = Olustur(Yillik, new SureIstegi(SureTipi.YarimGun, Pazartesi, null, GunYarisi.OgledenSonra, null, null), acikTalepler: [sabah]);

        Assert.Equal(240, talep.Dakika);
    }

    [Fact]
    public void Olustur_BakiyeYetersiz_KullanilabilirVeTalebiSoyler()
    {
        var hata = Assert.Throws<IsKuraliException>(() =>
            Olustur(Yillik, TamGun(Pazartesi, Pazartesi.AddDays(4)), bakiye: new Bakiye(3 * 480 + 240, 0, 0)));

        Assert.Equal("Bakiye yetersiz: kullanılabilir 3 gün 4 saat, talep 5 gün.", hata.Message);
    }

    [Fact]
    public void Olustur_BekleyenTalepBakiyeyiBlokeEder()
    {
        Assert.Throws<IsKuraliException>(() =>
            Olustur(Yillik, TamGun(Pazartesi, Pazartesi.AddDays(1)), bakiye: new Bakiye(14 * 480, 10 * 480, 3 * 480)));
    }

    [Fact]
    public void Olustur_BakiyedenDusmeyenTur_BakiyeyeBakmaz()
    {
        var ucretsiz = new IzinTuru("Ücretsiz", false, false, GunHesabi.IsGunu, true, true, false, null, null);

        var talep = Olustur(ucretsiz, TamGun(Pazartesi, Pazartesi.AddDays(4)), bakiye: new Bakiye(0, 0, 0));

        Assert.Equal(2400, talep.Dakika);
    }

    [Fact]
    public void Olustur_GecmisTarihYetkisiz_IsKuraliHatasiVerir()
    {
        var hata = Assert.Throws<IsKuraliException>(() =>
            Olustur(Yillik, TamGun(Pazartesi, Pazartesi), simdi: new DateTime(2026, 3, 2, 10, 0, 0)));

        Assert.Equal("Geçmiş tarihli izin talebi açılamaz.", hata.Message);
    }

    [Fact]
    public void Olustur_GecmisTarihDuzeltYetkisiyle_Olusur()
    {
        var talep = Olustur(Yillik, TamGun(Pazartesi, Pazartesi), simdi: new DateTime(2026, 3, 9, 10, 0, 0), gecmisTarihYetkisi: true);

        Assert.Equal(480, talep.Dakika);
    }

    [Fact]
    public void Olustur_PasifTur_IsKuraliHatasiVerir()
    {
        var pasif = new IzinTuru("Eski Tür", false, false, GunHesabi.IsGunu, true, false, false, null, null);
        pasif.AktiflikDegistir(false);

        Assert.Throws<IsKuraliException>(() => Olustur(pasif, TamGun(Pazartesi, Pazartesi)));
    }

    private static SureIstegi TamGun(DateOnly baslangic, DateOnly bitis) => new(SureTipi.TamGun, baslangic, bitis, null, null, null);

    private static IzinTalebi Olustur(
        IzinTuru tur,
        SureIstegi sure,
        IReadOnlyCollection<ZamanAraligi>? acikTalepler = null,
        Bakiye? bakiye = null,
        int turunYilIcindeKullanimi = 0,
        DateTime? simdi = null,
        bool gecmisTarihYetkisi = false) =>
        IzinTalebi.Olustur(
            1, tur, sure, null, TestTakvimi.Olustur(), acikTalepler ?? [], bakiye ?? OnDortGun,
            turunYilIcindeKullanimi, simdi ?? Simdi, gecmisTarihYetkisi);
}
