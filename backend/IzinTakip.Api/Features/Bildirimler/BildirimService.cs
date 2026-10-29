using IzinTakip.Api.Common.Paging;
using IzinTakip.Domain.Entities;
using IzinTakip.Domain.Exceptions;
using IzinTakip.Infrastructure.Data;
using IzinTakip.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace IzinTakip.Api.Features.Bildirimler;

public sealed class BildirimService(IzinTakipDbContext db, ICurrentUser gecerliKullanici)
{
    public async Task<PagedResponse<BildirimResponse>> ListeleAsync(BildirimListeRequest request, CancellationToken cancellationToken)
    {
        var sorgu = (await BenimkilerAsync(cancellationToken)).AsNoTracking();
        if (request.SadeceOkunmamis)
        {
            sorgu = sorgu.Where(b => !b.OkunduMu);
        }

        return await sorgu
            .OrderByDescending(b => b.Id)
            .Select(b => new BildirimResponse(b.Id, b.Baslik, b.Metin, b.Baglanti, b.OkunduMu, b.OlusturmaTarihi))
            .SayfalaAsync(request.Page, request.PageSize, cancellationToken);
    }

    public async Task<BildirimSayisiResponse> SayiAsync(CancellationToken cancellationToken) =>
        new(await (await BenimkilerAsync(cancellationToken)).CountAsync(b => !b.OkunduMu, cancellationToken));

    public async Task OkunduIsaretleAsync(int id, CancellationToken cancellationToken)
    {
        var bildirim = await (await BenimkilerAsync(cancellationToken)).SingleOrDefaultAsync(b => b.Id == id, cancellationToken)
            ?? throw new NotFoundException("Bildirim bulunamadı.");
        bildirim.OkunduIsaretle();
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task TumunuOkunduIsaretleAsync(CancellationToken cancellationToken)
    {
        var okunmamislar = await (await BenimkilerAsync(cancellationToken)).Where(b => !b.OkunduMu).ToListAsync(cancellationToken);
        foreach (var bildirim in okunmamislar)
        {
            bildirim.OkunduIsaretle();
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<IQueryable<Bildirim>> BenimkilerAsync(CancellationToken cancellationToken)
    {
        var kullaniciId = (await gecerliKullanici.YetkileriAsync(cancellationToken)).KullaniciId;
        return db.Bildirimler.Where(b => b.KullaniciId == kullaniciId);
    }
}
