namespace IzinTakip.Infrastructure.Identity;

public interface ICurrentUser
{
    int? KullaniciId { get; }

    Task<KullaniciYetkileri> YetkileriAsync(CancellationToken cancellationToken);
}
