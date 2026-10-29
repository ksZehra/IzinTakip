using IzinTakip.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IzinTakip.Api.Common.Izin;

public static class AyarlarSorgusu
{
    public static async Task<int> GunlukDakikaAsync(this IzinTakipDbContext db, CancellationToken cancellationToken) =>
        (await db.Ayarlar.AsNoTracking().SingleAsync(cancellationToken)).GunlukCalismaDakikasi;
}
