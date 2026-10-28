using IzinTakip.Domain.Entities;
using IzinTakip.Domain.Enums;
using IzinTakip.Domain.Exceptions;
using IzinTakip.Domain.Izin;
using static IzinTakip.Tests.Domain.TestTakvimi;

namespace IzinTakip.Tests.Domain;

public sealed class IzinTalebiDurumTests
{
    private const int TalepSahibi = 1;
    private const int Yonetici = 2;
    private static readonly DateTime Simdi = new(2026, 2, 2, 10, 0, 0);
    private static readonly DateTime KararZamani = new(2026, 2, 2, 7, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Onayla_Beklemede_OnaylanirVeKarariYazar()
    {
        var talep = YeniTalep();

        talep.Onayla(20, Yonetici, false, "  İyi tatiller  ", KararZamani);

        Assert.Equal(IzinDurumu.Onaylandi, talep.Durum);
        Assert.Equal(20, talep.KararVerenKullaniciId);
        Assert.Equal("İyi tatiller", talep.KararNotu);
        Assert.Equal(KararZamani, talep.KararTarihi);
    }

    [Fact]
    public void Onayla_KendiTalebi_IsKuraliHatasiVerir()
    {
        var hata = Assert.Throws<IsKuraliException>(() => YeniTalep().Onayla(10, TalepSahibi, false, null, KararZamani));

        Assert.Equal("Kendi izin talebinizi onaylayamazsınız.", hata.Message);
    }

    [Fact]
    public void Onayla_SuperAdminKendiTalebi_Onaylanir()
    {
        var talep = YeniTalep();

        talep.Onayla(10, TalepSahibi, true, null, KararZamani);

        Assert.Equal(IzinDurumu.Onaylandi, talep.Durum);
        Assert.Null(talep.KararNotu);
    }

    [Fact]
    public void Onayla_KararVerilmisTalep_IsKuraliHatasiVerir()
    {
        var talep = YeniTalep();
        talep.Reddet(20, Yonetici, false, "Yoğun dönem", KararZamani);

        var hata = Assert.Throws<IsKuraliException>(() => talep.Onayla(20, Yonetici, false, null, KararZamani));

        Assert.Equal("Bu talep artık beklemede değil.", hata.Message);
    }

    [Fact]
    public void Reddet_GerekceYok_IsKuraliHatasiVerir()
    {
        var hata = Assert.Throws<IsKuraliException>(() => YeniTalep().Reddet(20, Yonetici, false, " ", KararZamani));

        Assert.Equal("Red gerekçesi zorunludur.", hata.Message);
    }

    [Fact]
    public void Reddet_KendiTalebi_IsKuraliHatasiVerir()
    {
        var hata = Assert.Throws<IsKuraliException>(() => YeniTalep().Reddet(10, TalepSahibi, false, "Gerekçe", KararZamani));

        Assert.Equal("Kendi izin talebinizi reddedemezsiniz.", hata.Message);
    }

    [Fact]
    public void IptalEt_OnaylanmisVeBaslamamis_IptalEdilir()
    {
        var talep = YeniTalep();
        talep.Onayla(20, Yonetici, false, null, KararZamani);

        talep.IptalEt(10, "Plan değişti", Simdi, KararZamani, false);

        Assert.Equal(IzinDurumu.IptalEdildi, talep.Durum);
        Assert.Equal("Plan değişti", talep.KararNotu);
    }

    [Fact]
    public void IptalEt_Reddedilmis_IsKuraliHatasiVerir()
    {
        var talep = YeniTalep();
        talep.Reddet(20, Yonetici, false, "Yoğun dönem", KararZamani);

        var hata = Assert.Throws<IsKuraliException>(() => talep.IptalEt(10, null, Simdi, KararZamani, false));

        Assert.Equal("Yalnız beklemedeki ya da onaylanmış talep iptal edilebilir.", hata.Message);
    }

    [Fact]
    public void IptalEt_BaslamisIzinYetkisiz_IsKuraliHatasiVerir()
    {
        var baslamisken = Pazartesi.ToDateTime(new TimeOnly(11, 0));

        var hata = Assert.Throws<IsKuraliException>(() => YeniTalep().IptalEt(10, null, baslamisken, KararZamani, false));

        Assert.Equal("Başlamış ya da geçmiş izin iptal edilemez.", hata.Message);
    }

    [Fact]
    public void IptalEt_BaslamisIzinDuzeltYetkisiyle_IptalEdilir()
    {
        var talep = YeniTalep();

        talep.IptalEt(30, null, Pazartesi.ToDateTime(new TimeOnly(11, 0)), KararZamani, true);

        Assert.Equal(IzinDurumu.IptalEdildi, talep.Durum);
    }

    private static IzinTalebi YeniTalep() =>
        IzinTalebi.Olustur(
            TalepSahibi,
            new IzinTuru("Yıllık", true, false, GunHesabi.IsGunu, true, true, false, null, null),
            new SureIstegi(SureTipi.TamGun, Pazartesi, Pazartesi, null, null, null),
            null,
            TestTakvimi.Olustur(),
            [],
            new Bakiye(14 * 480, 0, 0),
            0,
            Simdi,
            false);
}
