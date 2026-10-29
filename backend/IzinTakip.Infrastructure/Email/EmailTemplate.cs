using System.Net;

namespace IzinTakip.Infrastructure.Email;

public static class EmailTemplate
{
    private static readonly string BildirimSablonu;

    static EmailTemplate()
    {
        using var akis = typeof(EmailTemplate).Assembly.GetManifestResourceStream("IzinTakip.Infrastructure.Email.Templates.Bildirim.html")
            ?? throw new InvalidOperationException("Bildirim e-posta şablonu bulunamadı.");
        using var okuyucu = new StreamReader(akis);
        BildirimSablonu = okuyucu.ReadToEnd();
    }

    public static string Bildirim(string baslik, string metin, string baglanti, string baglantiMetni) =>
        BildirimSablonu
            .Replace("{{Baslik}}", WebUtility.HtmlEncode(baslik), StringComparison.Ordinal)
            .Replace("{{Metin}}", WebUtility.HtmlEncode(metin), StringComparison.Ordinal)
            .Replace("{{Baglanti}}", WebUtility.HtmlEncode(baglanti), StringComparison.Ordinal)
            .Replace("{{BaglantiMetni}}", WebUtility.HtmlEncode(baglantiMetni), StringComparison.Ordinal);
}
