using IzinTakip.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IzinTakip.Api.Common;

public sealed class KullaniciAdlari(IzinTakipDbContext db)
{
    public async Task<IReadOnlyDictionary<int, string>> OkuAsync(IEnumerable<int?> kullaniciIdleri, CancellationToken cancellationToken)
    {
        var idler = kullaniciIdleri.OfType<int>().Distinct().ToList();
        return await db.Users
            .AsNoTracking()
            .Where(k => idler.Contains(k.Id))
            .Select(k => new
            {
                k.Id,
                Ad = db.Calisanlar.Where(c => c.Id == k.CalisanId).Select(c => c.Ad + " " + c.Soyad).FirstOrDefault() ?? k.Email!
            })
            .ToDictionaryAsync(x => x.Id, x => x.Ad, cancellationToken);
    }
}
