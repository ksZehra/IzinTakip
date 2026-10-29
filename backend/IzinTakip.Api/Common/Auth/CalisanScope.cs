using IzinTakip.Domain;
using IzinTakip.Domain.Entities;
using IzinTakip.Domain.Exceptions;
using IzinTakip.Infrastructure.Data;
using IzinTakip.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace IzinTakip.Api.Common.Auth;

public sealed class CalisanScope(IzinTakipDbContext db, ICurrentUser gecerliKullanici)
{
    public async Task<IQueryable<Calisan>> CalisanlarAsync(CancellationToken cancellationToken)
    {
        var yetkiler = await gecerliKullanici.YetkileriAsync(cancellationToken);
        if (yetkiler.VarMi(Yetkiler.TumSirket))
        {
            return db.Calisanlar;
        }

        var calisanId = yetkiler.CalisanId;
        if (calisanId is null)
        {
            return db.Calisanlar.Where(_ => false);
        }

        if (!yetkiler.VarMi(Yetkiler.IzinTalebi.Yonet))
        {
            return db.Calisanlar.Where(c => c.Id == calisanId);
        }

        return db.Calisanlar.Where(c =>
            c.Id == calisanId || db.Departmanlar.Any(d => d.Id == c.DepartmanId && d.YoneticiId == calisanId));
    }

    public async Task<Calisan> BulAsync(int calisanId, CancellationToken cancellationToken) =>
        await (await CalisanlarAsync(cancellationToken)).SingleOrDefaultAsync(c => c.Id == calisanId, cancellationToken)
        ?? throw new NotFoundException("Çalışan bulunamadı.");
}
