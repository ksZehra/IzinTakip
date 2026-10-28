using IzinTakip.Domain.Entities;
using IzinTakip.Domain.Exceptions;
using IzinTakip.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IzinTakip.Api.Features.HakKurallari;

public sealed class HakKuraliService(IzinTakipDbContext db, ILogger<HakKuraliService> logger)
{
    public async Task<IReadOnlyList<HakKuraliResponse>> ListeleAsync(CancellationToken cancellationToken) =>
        await db.IzinHakkiKurallari
            .AsNoTracking()
            .OrderBy(k => k.MinKidemYil).ThenBy(k => k.Id)
            .Select(k => new HakKuraliResponse(k.Id, k.Ad, k.MinKidemYil, k.MaxKidemYil, k.MinYas, k.MaxYas, k.Gun, k.Aktif))
            .ToListAsync(cancellationToken);

    public async Task<HakKuraliResponse> DetayAsync(int id, CancellationToken cancellationToken) =>
        Yanit(await BulAsync(id, cancellationToken));

    public async Task<HakKuraliResponse> OlusturAsync(HakKuraliKaydetRequest request, CancellationToken cancellationToken)
    {
        var kural = new IzinHakkiKurali(request.Ad.Trim(), request.MinKidemYil, request.MaxKidemYil, request.MinYas, request.MaxYas, request.Gun);
        db.IzinHakkiKurallari.Add(kural);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Hak kurali olusturuldu {KuralId}", kural.Id);
        return Yanit(kural);
    }

    public async Task<HakKuraliResponse> GuncelleAsync(int id, HakKuraliKaydetRequest request, CancellationToken cancellationToken)
    {
        var kural = await BulAsync(id, cancellationToken);
        kural.Guncelle(request.Ad.Trim(), request.MinKidemYil, request.MaxKidemYil, request.MinYas, request.MaxYas, request.Gun);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Hak kurali guncellendi {KuralId}", id);
        return Yanit(kural);
    }

    public async Task<HakKuraliResponse> AktiflikDegistirAsync(int id, bool aktif, CancellationToken cancellationToken)
    {
        var kural = await BulAsync(id, cancellationToken);
        kural.AktiflikDegistir(aktif);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Hak kurali aktifligi degisti {KuralId} {Aktif}", id, aktif);
        return Yanit(kural);
    }

    private async Task<IzinHakkiKurali> BulAsync(int id, CancellationToken cancellationToken) =>
        await db.IzinHakkiKurallari.SingleOrDefaultAsync(k => k.Id == id, cancellationToken)
        ?? throw new KayitBulunamadiException("Hak kuralı bulunamadı.");

    private static HakKuraliResponse Yanit(IzinHakkiKurali k) =>
        new(k.Id, k.Ad, k.MinKidemYil, k.MaxKidemYil, k.MinYas, k.MaxYas, k.Gun, k.Aktif);
}
