namespace IzinTakip.Infrastructure.Email;

public interface IEmailSender
{
    Task SendAsync(EmailMessage mesaj, CancellationToken cancellationToken);
}
