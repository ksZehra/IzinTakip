using IzinTakip.Domain.Enums;
using IzinTakip.Domain.Exceptions;
using IzinTakip.Domain.Izin;
using static IzinTakip.Tests.Domain.TestTakvimi;

namespace IzinTakip.Tests.Domain;

public sealed class IzinSuresiHesaplayiciTests
{
    private readonly CalismaTakvimi _takvim = Olustur();

    [Fact]
    public void Hesapla_TamGunHaftaIciBesGun_IkiBinDortYuzDakika()
    {
        var sonuc = TamGun(Pazartesi, Pazartesi.AddDays(4));

        Assert.Equal(2400, sonuc.Dakika);
        Assert.Equal(new DateTime(2026, 3, 2, 9, 0, 0), sonuc.Aralik.Baslangic);
        Assert.Equal(new DateTime(2026, 3, 6, 18, 0, 0), sonuc.Aralik.Bitis);
    }

    [Fact]
    public void Hesapla_TamGunHaftaSonuDahil_YalnizCalismaGunleriniSayar() =>
        Assert.Equal(960, TamGun(Pazartesi.AddDays(4), Pazartesi.AddDays(7)).Dakika);

    [Fact]
    public void Hesapla_TamGunArifeVeTatilIcerenHafta_ArifeyiYarimTatiliSifirSayar() =>
        Assert.Equal(480 + 480 + 240 + 0 + 480, TamGun(new DateOnly(2026, 10, 26), new DateOnly(2026, 10, 30)).Dakika);

    [Fact]
    public void Hesapla_TamGunTakvimGunu_HaftaSonunuDaSayar() =>
        Assert.Equal(4 * 480, TamGun(Pazartesi.AddDays(4), Pazartesi.AddDays(7), GunHesabi.TakvimGunu).Dakika);

    [Fact]
    public void Hesapla_TamGunYalnizHaftaSonu_IsKuraliHatasiVerir()
    {
        var hata = Assert.Throws<IsKuraliException>(() => TamGun(Pazartesi.AddDays(5), Pazartesi.AddDays(6)));

        Assert.Equal("Seçilen aralıkta çalışma günü yok.", hata.Message);
    }

    [Fact]
    public void Hesapla_YarimGunSabah_GununYarisiVeOgleArasinaKadarAralik()
    {
        var sonuc = IzinSuresiHesaplayici.Hesapla(new SureIstegi(SureTipi.YarimGun, Pazartesi, null, GunYarisi.Sabah, null, null), GunHesabi.IsGunu, _takvim);

        Assert.Equal(240, sonuc.Dakika);
        Assert.Equal(new TimeOnly(12, 30), TimeOnly.FromDateTime(sonuc.Aralik.Bitis));
    }

    [Fact]
    public void Hesapla_YarimGunOgledenSonra_OgleArasiBitisindenMesaiBitisine()
    {
        var sonuc = IzinSuresiHesaplayici.Hesapla(new SureIstegi(SureTipi.YarimGun, Pazartesi, null, GunYarisi.OgledenSonra, null, null), GunHesabi.IsGunu, _takvim);

        Assert.Equal(240, sonuc.Dakika);
        Assert.Equal(new TimeOnly(13, 30), TimeOnly.FromDateTime(sonuc.Aralik.Baslangic));
        Assert.Equal(new TimeOnly(18, 0), TimeOnly.FromDateTime(sonuc.Aralik.Bitis));
    }

    [Fact]
    public void Hesapla_ArifedeOgledenSonra_IsKuraliHatasiVerir()
    {
        var hata = Assert.Throws<IsKuraliException>(() =>
            IzinSuresiHesaplayici.Hesapla(new SureIstegi(SureTipi.YarimGun, Arife, null, GunYarisi.OgledenSonra, null, null), GunHesabi.IsGunu, _takvim));

        Assert.Equal("Bu gün öğleden sonra zaten resmi tatil.", hata.Message);
    }

    [Fact]
    public void Hesapla_YarimGunHaftaSonu_IsKuraliHatasiVerir() =>
        Assert.Throws<IsKuraliException>(() =>
            IzinSuresiHesaplayici.Hesapla(new SureIstegi(SureTipi.YarimGun, Pazartesi.AddDays(5), null, GunYarisi.Sabah, null, null), GunHesabi.IsGunu, _takvim));

    [Fact]
    public void Hesapla_SaatlikOgleArasiniKapsayan_OgleArasiniDuser() =>
        Assert.Equal(120, Saatlik(Pazartesi, new(11, 0), new(14, 0)).Dakika);

    [Fact]
    public void Hesapla_SaatlikMesaiDisinda_IsKuraliHatasiVerir() =>
        Assert.Throws<IsKuraliException>(() => Saatlik(Pazartesi, new(8, 0), new(10, 0)));

    [Fact]
    public void Hesapla_SaatlikAdimDisinda_IsKuraliHatasiVerir()
    {
        var hata = Assert.Throws<IsKuraliException>(() => Saatlik(Pazartesi, new(10, 15), new(11, 0)));

        Assert.Equal("Saatlik izin 30 dakikalık adımlarla seçilir.", hata.Message);
    }

    [Fact]
    public void Hesapla_SaatlikTamamenOgleArasinda_IsKuraliHatasiVerir() =>
        Assert.Throws<IsKuraliException>(() => Saatlik(Pazartesi, new(12, 30), new(13, 30)));

    [Fact]
    public void Hesapla_ArifedeOgledenSonraSaatlik_IsKuraliHatasiVerir() =>
        Assert.Throws<IsKuraliException>(() => Saatlik(Arife, new(14, 0), new(15, 0)));

    [Fact]
    public void Hesapla_ArifedeSabahSaatlik_Hesaplanir() =>
        Assert.Equal(90, Saatlik(Arife, new(10, 0), new(11, 30)).Dakika);

    private IzinSuresi TamGun(DateOnly baslangic, DateOnly bitis, GunHesabi gunHesabi = GunHesabi.IsGunu) =>
        IzinSuresiHesaplayici.Hesapla(new SureIstegi(SureTipi.TamGun, baslangic, bitis, null, null, null), gunHesabi, _takvim);

    private IzinSuresi Saatlik(DateOnly tarih, TimeOnly baslangic, TimeOnly bitis) =>
        IzinSuresiHesaplayici.Hesapla(new SureIstegi(SureTipi.Saatlik, tarih, null, null, baslangic, bitis), GunHesabi.IsGunu, _takvim);
}
