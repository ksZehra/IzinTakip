using IzinTakip.Api.Common;
using IzinTakip.Api.Common.Auth;
using IzinTakip.Domain.Entities;
using IzinTakip.Domain.Exceptions;
using IzinTakip.Infrastructure.Data;
using IzinTakip.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;

namespace IzinTakip.Api.Features.IzinBelgeleri;

public sealed class IzinBelgesiService(
    IzinTakipDbContext db,
    CalisanScope kapsam,
    IFileStorage fileStorage,
    ILogger<IzinBelgesiService> logger)
{
    public async Task<IReadOnlyList<IzinBelgesiResponse>> ListeleAsync(int talepId, CancellationToken cancellationToken)
    {
        await KapsamdakiTalepAsync(talepId, cancellationToken);
        return await Yansit(db.IzinBelgeleri.AsNoTracking().Where(b => b.IzinTalebiId == talepId).OrderBy(b => b.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<IzinBelgesiResponse> YukleAsync(int talepId, IFormFile dosya, CancellationToken cancellationToken)
    {
        var talep = await KapsamdakiTalepAsync(talepId, cancellationToken);
        talep.BelgeEklemeyiDenetle(await db.IzinBelgeleri.CountAsync(b => b.IzinTalebiId == talepId, cancellationToken));

        var uzanti = Path.GetExtension(dosya.FileName).ToLowerInvariant();
        await using var akis = dosya.OpenReadStream();
        var depoYolu = await fileStorage.KaydetAsync(akis, uzanti, cancellationToken);
        var dosyaAdi = Path.GetFileName(dosya.FileName).Trim();
        var belge = new IzinBelgesi(
            talepId,
            dosyaAdi.Length <= 200 ? dosyaAdi : dosyaAdi[^200..],
            depoYolu,
            IzinBelgesiYukleRequestValidator.IcerikTurleri[uzanti],
            dosya.Length);
        db.IzinBelgeleri.Add(belge);
        RowVersionIleKoru(talep);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await fileStorage.SilAsync(depoYolu, CancellationToken.None);
            throw;
        }

        logger.LogInformation("Izin belgesi yuklendi {IzinBelgesiId} {IzinTalebiId} {Boyut}", belge.Id, talepId, belge.Boyut);
        return await Yansit(db.IzinBelgeleri.AsNoTracking().Where(b => b.Id == belge.Id)).SingleAsync(cancellationToken);
    }

    public async Task<DosyaResponse> IndirAsync(int talepId, int id, CancellationToken cancellationToken)
    {
        await KapsamdakiTalepAsync(talepId, cancellationToken);
        var belge = await BelgeAsync(talepId, id, cancellationToken);
        return new DosyaResponse(belge.DosyaAdi, belge.IcerikTuru, await fileStorage.OkuAsync(belge.DepoYolu, cancellationToken));
    }

    public async Task SilAsync(int talepId, int id, CancellationToken cancellationToken)
    {
        var talep = await KapsamdakiTalepAsync(talepId, cancellationToken);
        talep.BelgeSilmeyiDenetle();
        var belge = await BelgeAsync(talepId, id, cancellationToken);

        db.IzinBelgeleri.Remove(belge);
        RowVersionIleKoru(talep);
        await db.SaveChangesAsync(cancellationToken);
        await fileStorage.SilAsync(belge.DepoYolu, cancellationToken);

        logger.LogInformation("Izin belgesi silindi {IzinBelgesiId} {IzinTalebiId}", id, talepId);
    }

    private async Task<IzinTalebi> KapsamdakiTalepAsync(int talepId, CancellationToken cancellationToken)
    {
        var calisanlar = await kapsam.CalisanlarAsync(cancellationToken);
        return await db.IzinTalepleri
            .SingleOrDefaultAsync(t => t.Id == talepId && calisanlar.Any(c => c.Id == t.CalisanId), cancellationToken)
            ?? throw new NotFoundException("İzin talebi bulunamadı.");
    }

    private void RowVersionIleKoru(IzinTalebi talep) => db.Entry(talep).State = EntityState.Modified;

    private async Task<IzinBelgesi> BelgeAsync(int talepId, int id, CancellationToken cancellationToken) =>
        await db.IzinBelgeleri.SingleOrDefaultAsync(b => b.Id == id && b.IzinTalebiId == talepId, cancellationToken)
        ?? throw new NotFoundException("Belge bulunamadı.");

    private static IQueryable<IzinBelgesiResponse> Yansit(IQueryable<IzinBelgesi> belgeler) =>
        belgeler.Select(b => new IzinBelgesiResponse(b.Id, b.DosyaAdi, b.IcerikTuru, b.Boyut, b.OlusturmaTarihi));
}
