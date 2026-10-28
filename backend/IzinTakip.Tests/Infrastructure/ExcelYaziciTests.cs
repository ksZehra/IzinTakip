using ClosedXML.Excel;
using IzinTakip.Infrastructure.Excel;

namespace IzinTakip.Tests.Infrastructure;

public sealed class ExcelYaziciTests
{
    [Fact]
    public void Yaz_BaslikVeSatirlar_HucrelereBicimiyleYazilir()
    {
        var icerik = ExcelYazici.Yaz(
            "Liste",
            [
                new ExcelKolonu<Satir>("Ad", s => s.Ad),
                new ExcelKolonu<Satir>("Tarih", s => s.Tarih, 12, "dd.MM.yyyy"),
                new ExcelKolonu<Satir>("Not", s => s.Not)
            ],
            [new Satir("Ayşe Yılmaz", new DateTime(2027, 3, 1), null), new Satir("Can Öz", new DateTime(2027, 3, 2), "Rapor")]);

        using var kitap = new XLWorkbook(new MemoryStream(icerik));
        var sayfa = kitap.Worksheet("Liste");

        Assert.Equal("Ad", sayfa.Cell(1, 1).GetString());
        Assert.True(sayfa.Cell(1, 1).Style.Font.Bold);
        Assert.Equal("Ayşe Yılmaz", sayfa.Cell(2, 1).GetString());
        Assert.Equal(new DateTime(2027, 3, 1), sayfa.Cell(2, 2).GetDateTime());
        Assert.Equal("dd.MM.yyyy", sayfa.Cell(2, 2).Style.NumberFormat.Format);
        Assert.True(sayfa.Cell(2, 3).IsEmpty());
        Assert.Equal("Rapor", sayfa.Cell(3, 3).GetString());
        Assert.True(sayfa.AutoFilter.IsEnabled);
    }

    [Fact]
    public void Yaz_SatirYok_YalnizBaslikYazilir()
    {
        var icerik = ExcelYazici.Yaz("Bos", [new ExcelKolonu<Satir>("Ad", s => s.Ad)], []);

        using var kitap = new XLWorkbook(new MemoryStream(icerik));

        Assert.Equal(1, kitap.Worksheet("Bos").LastRowUsed()!.RowNumber());
    }

    private sealed record Satir(string Ad, DateTime Tarih, string? Not);
}
