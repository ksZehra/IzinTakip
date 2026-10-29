using System.Threading.Channels;

namespace IzinTakip.Infrastructure.Email;

public sealed class ChannelEmailQueue : IEmailQueue
{
    private readonly Channel<EmailMessage> _kanal = Channel.CreateUnbounded<EmailMessage>(new UnboundedChannelOptions { SingleReader = true });

    public void Enqueue(EmailMessage mesaj) => _kanal.Writer.TryWrite(mesaj);

    public IAsyncEnumerable<EmailMessage> ReadAllAsync(CancellationToken cancellationToken) => _kanal.Reader.ReadAllAsync(cancellationToken);
}
