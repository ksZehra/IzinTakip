using IzinTakip.Api.Common.Bildirimler;
using IzinTakip.Domain;
using IzinTakip.Infrastructure.Data;
using IzinTakip.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace IzinTakip.Api.Common.Izin;

public sealed class OnayciBulucu(IzinTakipDbContext db)
{
    public async Task<IReadOnlyList<Alici>> OnaycilarAsync(int calisanId, CancellationToken cancellationToken)
    {
        var yoneticiId = await db.Calisanlar
            .Where(c => c.Id == calisanId)
            .Select(c => c.Departman.YoneticiId)
            .SingleAsync(cancellationToken);

        if (yoneticiId is not null && yoneticiId != calisanId)
        {
            var yonetici = await Alicilar(AktifKullanicilar().Where(k => k.CalisanId == yoneticiId), cancellationToken);
            if (yonetici.Count > 0)
            {
                return yonetici;
            }
        }

        var tumSirketRolleri = db.RoleClaims
            .Where(c => c.ClaimType == Yetkiler.ClaimType && c.ClaimValue == Yetkiler.TumSirket)
            .Select(c => c.RoleId);
        return await Alicilar(
            AktifKullanicilar().Where(k =>
                k.CalisanId != calisanId && db.UserRoles.Any(r => r.UserId == k.Id && tumSirketRolleri.Contains(r.RoleId))),
            cancellationToken);
    }

    public Task<IReadOnlyList<Alici>> CalisaninKendisiAsync(int calisanId, CancellationToken cancellationToken) =>
        Alicilar(AktifKullanicilar().Where(k => k.CalisanId == calisanId), cancellationToken);

    private IQueryable<Kullanici> AktifKullanicilar() => db.Users.Where(k => k.Aktif && k.Email != null);

    private static async Task<IReadOnlyList<Alici>> Alicilar(IQueryable<Kullanici> kullanicilar, CancellationToken cancellationToken) =>
        await kullanicilar.Select(k => new Alici(k.Id, k.Email!)).ToListAsync(cancellationToken);
}
