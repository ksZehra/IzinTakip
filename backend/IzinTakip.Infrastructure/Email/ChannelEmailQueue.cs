using System.Threading.Channels;

namespace IzinTakip.Infrastructure.Email;

public sealed class ChannelEmailQueue : IEmailQueue
{
    private readonly Channel<EmailMesaji> _kanal = Channel.CreateUnbounded<EmailMesaji>(new UnboundedChannelOptions { SingleReader = true });

    public void Enqueue(EmailMesaji mesaj) => _kanal.Writer.TryWrite(mesaj);

    public IAsyncEnumerable<EmailMesaji> ReadAllAsync(CancellationToken cancellationToken) => _kanal.Reader.ReadAllAsync(cancellationToken);
}
