namespace IzinTakip.Infrastructure.Email;

public sealed class EmailAyarlari
{
    public string Host { get; init; } = string.Empty;
    public int Port { get; init; } = 25;
    public bool StartTls { get; init; }
    public string? KullaniciAdi { get; init; }
    public string? Parola { get; init; }
    public string GonderenAdres { get; init; } = string.Empty;
    public string GonderenAd { get; init; } = string.Empty;
}
