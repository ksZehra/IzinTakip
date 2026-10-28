using IzinTakip.Domain.Entities;
using IzinTakip.Domain.Enums;
using IzinTakip.Domain.Izin;
using IzinTakip.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IzinTakip.Api.Common.Izin;

public sealed class BakiyeSorgusu(IzinTakipDbContext db)
{
    public async Task<Bakiye> HesaplaAsync(int calisanId, CancellationToken cancellationToken) =>
        (await HesaplaAsync(db.Calisanlar.Where(c => c.Id == calisanId), cancellationToken))[calisanId];

    public async Task<IReadOnlyDictionary<int, Bakiye>> HesaplaAsync(IQueryable<Calisan> calisanlar, CancellationToken cancellationToken)
    {
        var idler = await calisanlar.Select(c => c.Id).ToListAsync(cancellationToken);
        var haklar = await db.IzinHaklari
            .Where(h => calisanlar.Any(c => c.Id == h.CalisanId))
            .GroupBy(h => h.CalisanId)
            .Select(g => new { CalisanId = g.Key, Dakika = g.Sum(h => h.Dakika) })
            .ToDictionaryAsync(x => x.CalisanId, x => x.Dakika, cancellationToken);
        var talepler = await db.IzinTalepleri
            .Where(t => calisanlar.Any(c => c.Id == t.CalisanId)
                        && t.BakiyedenDuser
                        && (t.Durum == IzinDurumu.Onaylandi || t.Durum == IzinDurumu.Beklemede))
            .GroupBy(t => new { t.CalisanId, t.Durum })
            .Select(g => new { g.Key.CalisanId, g.Key.Durum, Dakika = g.Sum(t => t.Dakika) })
            .ToDictionaryAsync(x => (x.CalisanId, x.Durum), x => x.Dakika, cancellationToken);

        return idler.ToDictionary(id => id, id => new Bakiye(
            haklar.GetValueOrDefault(id),
            talepler.GetValueOrDefault((id, IzinDurumu.Onaylandi)),
            talepler.GetValueOrDefault((id, IzinDurumu.Beklemede))));
    }
}
