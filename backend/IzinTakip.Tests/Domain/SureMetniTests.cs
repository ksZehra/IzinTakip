using IzinTakip.Domain;

namespace IzinTakip.Tests.Domain;

public sealed class SureMetniTests
{
    [Theory]
    [InlineData(0, "0 gün")]
    [InlineData(480, "1 gün")]
    [InlineData(240, "yarım gün")]
    [InlineData(-240, "-yarım gün")]
    [InlineData(2400, "5 gün")]
    [InlineData(1110, "2 gün 2 saat 30 dk")]
    [InlineData(120, "2 saat")]
    [InlineData(45, "45 dk")]
    [InlineData(-960, "-2 gün")]
    public void Olustur_GunlukSekizSaatte_BeklenenMetniDoner(int dakika, string beklenen) =>
        Assert.Equal(beklenen, SureMetni.Olustur(dakika, 480));

    [Fact]
    public void Olustur_GunlukYediSaatte_GunuYediSaatSayar() =>
        Assert.Equal("1 gün 1 saat", SureMetni.Olustur(480, 420));
}
