using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace IzinTakip.Infrastructure.Email;

public sealed class MailKitEmailSender(EmailAyarlari ayarlar) : IEmailSender
{
    public async Task SendAsync(EmailMesaji mesaj, CancellationToken cancellationToken)
    {
        if (!MailboxAddress.TryParse(mesaj.Alici, out var alici))
        {
            throw new KaliciEmailHatasiException("Alici adresi gecersiz.");
        }

        using var mime = new MimeMessage();
        mime.From.Add(new MailboxAddress(ayarlar.GonderenAd, ayarlar.GonderenAdres));
        mime.To.Add(alici);
        mime.Subject = mesaj.Konu;
        mime.Body = new TextPart(TextFormat.Html) { Text = mesaj.HtmlGovde };

        using var istemci = new SmtpClient { Timeout = (int)TimeSpan.FromSeconds(ayarlar.ZamanAsimiSaniye).TotalMilliseconds };
        try
        {
            await istemci.ConnectAsync(
                ayarlar.Host,
                ayarlar.Port,
                ayarlar.StartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.None,
                cancellationToken);
            if (!string.IsNullOrEmpty(ayarlar.KullaniciAdi))
            {
                await istemci.AuthenticateAsync(ayarlar.KullaniciAdi, ayarlar.Parola ?? string.Empty, cancellationToken);
            }

            await istemci.SendAsync(mime, cancellationToken);
        }
        catch (Exception exception) when (exception is AuthenticationException or SmtpCommandException { StatusCode: >= (SmtpStatusCode)500 })
        {
            throw new KaliciEmailHatasiException("SMTP sunucusu istegi kalici olarak reddetti.", exception);
        }

        await istemci.DisconnectAsync(true, cancellationToken);
    }
}
