namespace IzinTakip.Infrastructure.Storage;

public interface IFileStorage
{
    Task<string> KaydetAsync(Stream icerik, string uzanti, CancellationToken cancellationToken);

    Task<byte[]> OkuAsync(string depoYolu, CancellationToken cancellationToken);

    Task SilAsync(string depoYolu, CancellationToken cancellationToken);
}
