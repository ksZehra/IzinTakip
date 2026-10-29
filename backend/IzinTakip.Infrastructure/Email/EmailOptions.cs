namespace IzinTakip.Infrastructure.Email;

public sealed class EmailOptions
{
    public string Host { get; init; } = string.Empty;
    public int Port { get; init; } = 587;
    public bool StartTls { get; init; } = true;
    public int ZamanAsimiSaniye { get; init; } = 30;
    public string? KullaniciAdi { get; init; }
    public string? Parola { get; init; }
    public string GonderenAdres { get; init; } = string.Empty;
    public string GonderenAd { get; init; } = string.Empty;
}
