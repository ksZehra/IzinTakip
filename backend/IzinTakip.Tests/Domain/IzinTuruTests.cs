using IzinTakip.Domain.Entities;
using IzinTakip.Domain.Enums;
using IzinTakip.Domain.Exceptions;

namespace IzinTakip.Tests.Domain;

public sealed class IzinTuruTests
{
    [Fact]
    public void Olustur_HicBicimSecilmemis_IsKuraliHatasiVerir() =>
        Assert.Throws<IsKuraliException>(() =>
            new IzinTuru("Deneme", false, false, GunHesabi.IsGunu, false, false, false, null, null));

    [Fact]
    public void Olustur_TakvimGunuVeYarimGun_IsKuraliHatasiVerir()
    {
        var hata = Assert.Throws<IsKuraliException>(() =>
            new IzinTuru("Deneme", false, false, GunHesabi.TakvimGunu, true, true, false, null, null));

        Assert.Equal("Takvim günü sayan izin türü yalnız tam gün alınabilir.", hata.Message);
    }

    [Fact]
    public void Olustur_TalepSiniriYillikSinirdanBuyuk_IsKuraliHatasiVerir() =>
        Assert.Throws<IsKuraliException>(() =>
            new IzinTuru("Deneme", false, false, GunHesabi.IsGunu, true, false, false, 2400, 960));

    [Fact]
    public void Olustur_GecerliTanim_AktifBaslar()
    {
        var tur = new IzinTuru("Mazeret", false, false, GunHesabi.IsGunu, true, true, true, 480, 2400);

        Assert.True(tur.Aktif);
        Assert.True(tur.SaatlikAlinir);
    }
}
