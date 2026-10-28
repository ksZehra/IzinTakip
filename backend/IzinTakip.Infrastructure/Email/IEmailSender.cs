namespace IzinTakip.Infrastructure.Email;

public interface IEmailSender
{
    Task SendAsync(EmailMesaji mesaj, CancellationToken cancellationToken);
}
