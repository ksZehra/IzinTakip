using IzinTakip.Domain;
using IzinTakip.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace IzinTakip.Infrastructure.Identity;

public sealed class KullaniciYetkiOkuyucu(IzinTakipDbContext db, IMemoryCache cache)
{
    private static readonly TimeSpan OnbellekSuresi = TimeSpan.FromMinutes(1);

    public async Task<KullaniciYetkileri?> OkuAsync(int kullaniciId, CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(Anahtar(kullaniciId), out KullaniciYetkileri? kayitli))
        {
            return kayitli;
        }

        var kullanici = await db.Users
            .AsNoTracking()
            .Where(k => k.Id == kullaniciId)
            .Select(k => new { k.Email, k.SecurityStamp, k.CalisanId, k.Aktif, k.ParolaDegistirmeliMi })
            .SingleOrDefaultAsync(cancellationToken);

        KullaniciYetkileri? sonuc = null;
        if (kullanici is not null)
        {
            var rolIdleri = db.UserRoles.Where(r => r.UserId == kullaniciId).Select(r => r.RoleId);
            var roller = await db.Roles
                .AsNoTracking()
                .Where(r => rolIdleri.Contains(r.Id))
                .Select(r => r.Name!)
                .ToListAsync(cancellationToken);
            var yetkiler = await db.RoleClaims
                .AsNoTracking()
                .Where(c => rolIdleri.Contains(c.RoleId) && c.ClaimType == Yetkiler.ClaimType)
                .Select(c => c.ClaimValue!)
                .Distinct()
                .ToListAsync(cancellationToken);

            sonuc = new KullaniciYetkileri(
                kullaniciId,
                kullanici.Email ?? string.Empty,
                kullanici.SecurityStamp ?? string.Empty,
                kullanici.CalisanId,
                kullanici.Aktif,
                kullanici.ParolaDegistirmeliMi,
                roller,
                yetkiler.ToHashSet());
        }

        cache.Set(Anahtar(kullaniciId), sonuc, OnbellekSuresi);
        return sonuc;
    }

    public void Unut(int kullaniciId) => cache.Remove(Anahtar(kullaniciId));

    private static string Anahtar(int kullaniciId) => $"kullanici-yetkileri:{kullaniciId}";
}
