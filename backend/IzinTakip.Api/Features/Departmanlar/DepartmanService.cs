using IzinTakip.Domain.Entities;
using IzinTakip.Domain.Exceptions;
using IzinTakip.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IzinTakip.Api.Features.Departmanlar;

public sealed class DepartmanService(IzinTakipDbContext db, ILogger<DepartmanService> logger)
{
    public async Task<IReadOnlyList<DepartmanResponse>> ListeleAsync(CancellationToken cancellationToken) =>
        await Yansit(db.Departmanlar.AsNoTracking().OrderBy(d => d.Ad)).ToListAsync(cancellationToken);

    public async Task<DepartmanResponse> DetayAsync(int id, CancellationToken cancellationToken) =>
        await Yansit(db.Departmanlar.AsNoTracking().Where(d => d.Id == id)).SingleOrDefaultAsync(cancellationToken)
        ?? throw new NotFoundException("Departman bulunamadı.");

    public async Task<DepartmanResponse> OlusturAsync(DepartmanKaydetRequest request, CancellationToken cancellationToken)
    {
        var ad = request.Ad.Trim();
        await DenetleAsync(null, ad, request.YoneticiId, cancellationToken);

        var departman = new Departman(ad, request.YoneticiId);
        db.Departmanlar.Add(departman);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Departman olusturuldu {DepartmanId}", departman.Id);
        return await DetayAsync(departman.Id, cancellationToken);
    }

    public async Task<DepartmanResponse> GuncelleAsync(int id, DepartmanKaydetRequest request, CancellationToken cancellationToken)
    {
        var departman = await db.Departmanlar.SingleOrDefaultAsync(d => d.Id == id, cancellationToken)
            ?? throw new NotFoundException("Departman bulunamadı.");
        var ad = request.Ad.Trim();
        await DenetleAsync(id, ad, request.YoneticiId, cancellationToken);

        departman.Guncelle(ad, request.YoneticiId);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Departman guncellendi {DepartmanId}", id);
        return await DetayAsync(id, cancellationToken);
    }

    public async Task<DepartmanResponse> AktiflikDegistirAsync(int id, bool aktif, CancellationToken cancellationToken)
    {
        var departman = await db.Departmanlar.SingleOrDefaultAsync(d => d.Id == id, cancellationToken)
            ?? throw new NotFoundException("Departman bulunamadı.");
        if (!aktif && await db.Calisanlar.AnyAsync(c => c.DepartmanId == id && c.Aktif, cancellationToken))
        {
            throw new BusinessRuleException("Departmanda aktif çalışan varken departman pasifleştirilemez.");
        }

        departman.AktiflikDegistir(aktif);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Departman aktifligi degisti {DepartmanId} {Aktif}", id, aktif);
        return await DetayAsync(id, cancellationToken);
    }

    private async Task DenetleAsync(int? id, string ad, int? yoneticiId, CancellationToken cancellationToken)
    {
        if (await db.Departmanlar.AnyAsync(d => d.Ad == ad && d.Id != id, cancellationToken))
        {
            throw new BusinessRuleException("Bu adla bir departman zaten var.");
        }

        if (yoneticiId is not null && !await db.Calisanlar.AnyAsync(c => c.Id == yoneticiId && c.Aktif, cancellationToken))
        {
            throw new BusinessRuleException("Seçilen yönetici bulunamadı ya da pasif.");
        }
    }

    private IQueryable<DepartmanResponse> Yansit(IQueryable<Departman> departmanlar) =>
        departmanlar.Select(d => new DepartmanResponse(
            d.Id,
            d.Ad,
            d.YoneticiId,
            db.Calisanlar.Where(c => c.Id == d.YoneticiId).Select(c => c.Ad + " " + c.Soyad).FirstOrDefault(),
            d.Aktif,
            db.Calisanlar.Count(c => c.DepartmanId == d.Id && c.Aktif)));
}
