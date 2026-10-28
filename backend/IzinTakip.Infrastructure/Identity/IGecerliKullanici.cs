namespace IzinTakip.Infrastructure.Identity;

public interface IGecerliKullanici
{
    int? KullaniciId { get; }

    Task<KullaniciYetkileri> YetkileriAsync(CancellationToken cancellationToken);
}
