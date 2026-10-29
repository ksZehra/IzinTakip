using IzinTakip.Domain.Izin;
using IzinTakip.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IzinTakip.Api.Common.Izin;

public sealed class CalismaTakvimiProvider(IzinTakipDbContext db)
{
    public async Task<CalismaTakvimi> OkuAsync(DateOnly baslangic, DateOnly bitis, CancellationToken cancellationToken)
    {
        var ayarlar = await db.Ayarlar.AsNoTracking().SingleAsync(cancellationToken);
        var tatiller = await db.ResmiTatiller
            .AsNoTracking()
            .Where(t => t.Tarih >= baslangic && t.Tarih <= bitis)
            .ToListAsync(cancellationToken);
        return CalismaTakvimi.Olustur(ayarlar, tatiller);
    }
}
