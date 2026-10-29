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
    private static readonly IzinTuru Saatlik = new("Saatlik", true, false, GunHesabi.IsGunu, false, false, true, null, null);

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
    public void Olustur_YillikSinirliTurdeYilSonunuAsan_IsKuraliHatasiVerir()
    {
        var mazeret = new IzinTuru("Mazeret", false, false, GunHesabi.IsGunu, true, true, false, null, 5 * 480);

        var hata = Assert.Throws<IsKuraliException>(() => Olustur(mazeret, TamGun(new(2026, 12, 30), new(2027, 1, 5))));

        Assert.Equal("Mazeret türünde yıllık sınır olduğu için talep yıl sonunu aşamaz. Talebi 31 Aralık'a kadar ve yeni yıl için iki ayrı talep olarak açın.", hata.Message);
    }

    [Fact]
    public void Olustur_YillikSinirliTurdeYilIcindeKalan_Olusur()
    {
        var mazeret = new IzinTuru("Mazeret", false, false, GunHesabi.IsGunu, true, true, false, null, 5 * 480);

        Assert.Equal(960, Olustur(mazeret, TamGun(new(2026, 12, 30), new(2026, 12, 31))).Dakika);
        Assert.Equal(960, Olustur(mazeret, TamGun(new(2027, 1, 4), new(2027, 1, 5))).Dakika);
    }

    [Fact]
    public void Olustur_YillikSiniriOlmayanTur_YilSonunuAsabilir()
    {
        var talep = Olustur(Yillik, TamGun(new(2026, 12, 30), new(2027, 1, 5)));

        Assert.Equal(2400, talep.Dakika);
    }

    [Fact]
    public void Olustur_AcikTalepleCakisiyor_IsKuraliHatasiVerir()
    {
        var mevcut = new ZamanAraligi(Pazartesi.ToDateTime(new(9, 0)), Pazartesi.ToDateTime(new(12, 30)));

        Assert.Throws<IsKuraliException>(() => Olustur(Yillik, TamGun(Pazartesi, Pazartesi), acikTalepler: [mevcut]));
    }

    [Fact]
    public void Olustur_TamGunIzinliGune_EskiMesaiyleKalanSaatDisindaBileSaatlikAcilamaz()
    {
        var eskiMesaiyleTamGun = IzinTalebi.CakismaAraligi(
            SureTipi.TamGun, new ZamanAraligi(Pazartesi.ToDateTime(new(10, 0)), Pazartesi.ToDateTime(new(19, 0))));

        Assert.Throws<IsKuraliException>(() =>
            Olustur(Saatlik, new SureIstegi(SureTipi.Saatlik, Pazartesi, null, null, new(9, 0), new(10, 0)), acikTalepler: [eskiMesaiyleTamGun]));
    }

    [Fact]
    public void Olustur_TamGun_EskiMesaiyleKalanSaatlikIzinleCakisir()
    {
        var eskiMesaiyleSaatlik = new ZamanAraligi(Pazartesi.ToDateTime(new(18, 0)), Pazartesi.ToDateTime(new(19, 0)));

        Assert.Throws<IsKuraliException>(() => Olustur(Yillik, TamGun(Pazartesi, Pazartesi), acikTalepler: [eskiMesaiyleSaatlik]));
    }

    [Fact]
    public void Olustur_ArdArdaTamGunler_Cakismaz()
    {
        var pazartesi = IzinTalebi.CakismaAraligi(
            SureTipi.TamGun, new ZamanAraligi(Pazartesi.ToDateTime(new(9, 0)), Pazartesi.ToDateTime(new(18, 0))));

        Assert.Equal(480, Olustur(Yillik, TamGun(Pazartesi.AddDays(1), Pazartesi.AddDays(1)), acikTalepler: [pazartesi]).Dakika);
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
    public void Olustur_BakiyedenDusmeBilgisi_TurdenTalebeKopyalanir()
    {
        var ucretsiz = new IzinTuru("Ücretsiz", false, false, GunHesabi.IsGunu, true, true, false, null, null);

        Assert.True(Olustur(Yillik, TamGun(Pazartesi, Pazartesi)).BakiyedenDuser);
        Assert.False(Olustur(ucretsiz, TamGun(Pazartesi, Pazartesi)).BakiyedenDuser);
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
