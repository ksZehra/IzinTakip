using IzinTakip.Domain.Entities;
using IzinTakip.Domain.Enums;
using IzinTakip.Domain.Exceptions;

namespace IzinTakip.Tests.Domain;

public sealed class AyarlarTests
{
    private const HaftaGunleri HaftaIci = HaftaGunleri.Pazartesi | HaftaGunleri.Sali | HaftaGunleri.Carsamba | HaftaGunleri.Persembe | HaftaGunleri.Cuma;

    [Fact]
    public void GunlukCalismaDakikasi_MesaidenOgleArasiniDuser()
    {
        var ayarlar = new Ayarlar(HaftaIci, new(9, 0), new(18, 0), new(12, 30), new(13, 30), 30, 2);

        Assert.Equal(480, ayarlar.GunlukCalismaDakikasi);
    }

    [Fact]
    public void Olustur_OgleArasiMesaiDisinda_IsKuraliHatasiVerir()
    {
        var hata = Assert.Throws<IsKuraliException>(() =>
            new Ayarlar(HaftaIci, new(9, 0), new(18, 0), new(8, 0), new(9, 30), 30, 2));

        Assert.Contains("öğle arası", hata.Message);
    }

    [Fact]
    public void Olustur_CalismaGunuYok_IsKuraliHatasiVerir() =>
        Assert.Throws<IsKuraliException>(() =>
            new Ayarlar(HaftaGunleri.Yok, new(9, 0), new(18, 0), new(12, 30), new(13, 30), 30, 2));

    [Fact]
    public void Olustur_GunlukSureTekDakika_YarimGuneBolunemedigiIcinReddedilir() =>
        Assert.Throws<IsKuraliException>(() =>
            new Ayarlar(HaftaIci, new(9, 0), new(17, 1), new(12, 0), new(13, 0), 30, 2));
}
