using IzinTakip.Api.BackgroundServices;

namespace IzinTakip.Tests.BackgroundServices;

public sealed class GunlukGorevTests
{
    private static readonly TimeOnly Saat = new(0, 10);

    [Theory]
    [InlineData("2027-03-01 00:05", "2027-03-01 00:10")]
    [InlineData("2027-03-01 00:10", "2027-03-02 00:10")]
    [InlineData("2027-03-01 23:59", "2027-03-02 00:10")]
    public void SonrakiCalisma_SaateGore_BugunYaDaYarin(string simdi, string beklenen)
    {
        Assert.Equal(DateTime.Parse(beklenen), GunlukGorev.SonrakiCalisma(DateTime.Parse(simdi), Saat));
    }
}
