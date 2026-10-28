namespace IzinTakip.Infrastructure.Storage;

public sealed class LocalFileStorage(DosyaDeposuAyarlari ayarlar) : IFileStorage
{
    public async Task<string> KaydetAsync(Stream icerik, string uzanti, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(ayarlar.Klasor);
        var depoYolu = $"{Guid.NewGuid():N}{uzanti}";
        await using var dosya = new FileStream(TamYol(depoYolu), FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true);
        await icerik.CopyToAsync(dosya, cancellationToken);
        return depoYolu;
    }

    public Task<byte[]> OkuAsync(string depoYolu, CancellationToken cancellationToken) =>
        File.ReadAllBytesAsync(TamYol(depoYolu), cancellationToken);

    public Task SilAsync(string depoYolu, CancellationToken cancellationToken)
    {
        File.Delete(TamYol(depoYolu));
        return Task.CompletedTask;
    }

    private string TamYol(string depoYolu) => Path.Combine(ayarlar.Klasor, Path.GetFileName(depoYolu));
}
