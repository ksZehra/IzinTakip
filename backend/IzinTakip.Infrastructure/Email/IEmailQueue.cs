namespace IzinTakip.Infrastructure.Email;

public interface IEmailQueue
{
    void Enqueue(EmailMesaji mesaj);

    IAsyncEnumerable<EmailMesaji> ReadAllAsync(CancellationToken cancellationToken);
}
