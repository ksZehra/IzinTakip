using IzinTakip.Domain.Izin;

namespace IzinTakip.Tests.Domain;

public sealed class ZamanAraligiTests
{
    private static readonly DateTime Gun = new(2026, 3, 2);

    [Fact]
    public void CakisirMi_UclariDegenAraliklar_Cakismaz() =>
        Assert.False(Aralik(10, 11).CakisirMi(Aralik(11, 12)));

    [Fact]
    public void CakisirMi_IcIceAraliklar_Cakisir() =>
        Assert.True(Aralik(9, 18).CakisirMi(Aralik(11, 12)));

    [Fact]
    public void CakisirMi_KismenOrtusenAraliklar_Cakisir() =>
        Assert.True(Aralik(9, 12).CakisirMi(Aralik(11, 14)));

    [Fact]
    public void KesisimDakika_KismenOrtusme_OrtakDakikayiDoner() =>
        Assert.Equal(60, Aralik(9, 12).KesisimDakika(Aralik(11, 14)));

    [Fact]
    public void KesisimDakika_AyrikAraliklar_SifirDoner() =>
        Assert.Equal(0, Aralik(9, 10).KesisimDakika(Aralik(11, 12)));

    private static ZamanAraligi Aralik(int baslangicSaati, int bitisSaati) =>
        new(Gun.AddHours(baslangicSaati), Gun.AddHours(bitisSaati));
}
