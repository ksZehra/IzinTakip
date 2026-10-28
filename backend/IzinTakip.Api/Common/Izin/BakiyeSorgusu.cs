using IzinTakip.Domain.Enums;
using IzinTakip.Domain.Izin;
using IzinTakip.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IzinTakip.Api.Common.Izin;

public sealed class BakiyeSorgusu(IzinTakipDbContext db)
{
    public async Task<Bakiye> HesaplaAsync(int calisanId, CancellationToken cancellationToken)
    {
        var hak = await db.IzinHaklari.Where(h => h.CalisanId == calisanId).SumAsync(h => h.Dakika, cancellationToken);
        var talepler = db.IzinTalepleri.Where(t => t.CalisanId == calisanId && t.IzinTuru.BakiyedenDuser);
        var kullanilan = await talepler.Where(t => t.Durum == IzinDurumu.Onaylandi).SumAsync(t => t.Dakika, cancellationToken);
        var bekleyen = await talepler.Where(t => t.Durum == IzinDurumu.Beklemede).SumAsync(t => t.Dakika, cancellationToken);
        return new Bakiye(hak, kullanilan, bekleyen);
    }
}
