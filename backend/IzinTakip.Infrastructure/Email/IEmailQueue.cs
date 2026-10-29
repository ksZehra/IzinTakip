namespace IzinTakip.Infrastructure.Email;

public interface IEmailQueue
{
    void Enqueue(EmailMessage mesaj);

    IAsyncEnumerable<EmailMessage> ReadAllAsync(CancellationToken cancellationToken);
}
