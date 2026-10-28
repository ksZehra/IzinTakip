using IzinTakip.Api.Common;
using IzinTakip.Domain.Entities;
using IzinTakip.Domain.Exceptions;
using IzinTakip.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IzinTakip.Api.Features.ResmiTatiller;

public sealed class ResmiTatilService(IzinTakipDbContext db, SirketSaati sirketSaati, ILogger<ResmiTatilService> logger)
{
    public async Task<IReadOnlyList<ResmiTatilResponse>> ListeleAsync(int? yil, CancellationToken cancellationToken)
    {
        var secilenYil = yil ?? sirketSaati.Bugun().Year;
        return await db.ResmiTatiller
            .AsNoTracking()
            .Where(t => t.Tarih.Year == secilenYil)
            .OrderBy(t => t.Tarih)
            .Select(t => new ResmiTatilResponse(t.Id, t.Tarih, t.Ad, t.YarimGun))
            .ToListAsync(cancellationToken);
    }

    public async Task<ResmiTatilResponse> DetayAsync(int id, CancellationToken cancellationToken) =>
        Yanit(await BulAsync(id, cancellationToken));

    public async Task<ResmiTatilResponse> OlusturAsync(ResmiTatilKaydetRequest request, CancellationToken cancellationToken)
    {
        await TarihBosMuAsync(null, request.Tarih, cancellationToken);

        var tatil = new ResmiTatil(request.Tarih, request.Ad.Trim(), request.YarimGun);
        db.ResmiTatiller.Add(tatil);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Resmi tatil eklendi {ResmiTatilId} {Tarih}", tatil.Id, tatil.Tarih);
        return Yanit(tatil);
    }

    public async Task<ResmiTatilResponse> GuncelleAsync(int id, ResmiTatilKaydetRequest request, CancellationToken cancellationToken)
    {
        var tatil = await BulAsync(id, cancellationToken);
        await TarihBosMuAsync(id, request.Tarih, cancellationToken);

        tatil.Guncelle(request.Tarih, request.Ad.Trim(), request.YarimGun);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Resmi tatil guncellendi {ResmiTatilId}", id);
        return Yanit(tatil);
    }

    public async Task SilAsync(int id, CancellationToken cancellationToken)
    {
        db.ResmiTatiller.Remove(await BulAsync(id, cancellationToken));
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Resmi tatil silindi {ResmiTatilId}", id);
    }

    private async Task<ResmiTatil> BulAsync(int id, CancellationToken cancellationToken) =>
        await db.ResmiTatiller.SingleOrDefaultAsync(t => t.Id == id, cancellationToken)
        ?? throw new KayitBulunamadiException("Resmi tatil bulunamadı.");

    private static ResmiTatilResponse Yanit(ResmiTatil tatil) => new(tatil.Id, tatil.Tarih, tatil.Ad, tatil.YarimGun);

    private async Task TarihBosMuAsync(int? id, DateOnly tarih, CancellationToken cancellationToken)
    {
        if (await db.ResmiTatiller.AnyAsync(t => t.Tarih == tarih && t.Id != id, cancellationToken))
        {
            throw new IsKuraliException("Bu tarihte zaten bir resmi tatil kayıtlı.");
        }
    }
}
