using ClosedXML.Excel;

namespace IzinTakip.Infrastructure.Excel;

public sealed record ExcelKolonu<T>(string Baslik, Func<T, object?> Deger, double Genislik = 16, string? Bicim = null);

public static class ExcelYazici
{
    public const string IcerikTuru = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public static byte[] Yaz<T>(string sayfaAdi, IReadOnlyList<ExcelKolonu<T>> kolonlar, IEnumerable<T> satirlar)
    {
        using var kitap = new XLWorkbook();
        var sayfa = kitap.AddWorksheet(sayfaAdi);
        for (var k = 0; k < kolonlar.Count; k++)
        {
            var kolon = sayfa.Column(k + 1);
            kolon.Width = kolonlar[k].Genislik;
            if (kolonlar[k].Bicim is { } bicim)
            {
                kolon.Style.NumberFormat.Format = bicim;
            }

            sayfa.Cell(1, k + 1).Value = kolonlar[k].Baslik;
        }

        var satir = 2;
        foreach (var kayit in satirlar)
        {
            for (var k = 0; k < kolonlar.Count; k++)
            {
                sayfa.Cell(satir, k + 1).Value = XLCellValue.FromObject(kolonlar[k].Deger(kayit));
            }

            satir++;
        }

        sayfa.Row(1).Style.Font.Bold = true;
        sayfa.SheetView.FreezeRows(1);
        sayfa.Range(1, 1, Math.Max(satir - 1, 1), kolonlar.Count).SetAutoFilter();

        using var akis = new MemoryStream();
        kitap.SaveAs(akis);
        return akis.ToArray();
    }
}
