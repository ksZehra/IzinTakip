using IzinTakip.Api.Common.Bildirimler;
using IzinTakip.Domain;
using IzinTakip.Infrastructure.Data;
using IzinTakip.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace IzinTakip.Api.Common.Izin;

public sealed class OnayciBulucu(IzinTakipDbContext db)
{
    public async Task<IReadOnlyList<Alici>> OnaycilarAsync(int calisanId, CancellationToken cancellationToken) =>
        (await OnaycilarAsync([calisanId], cancellationToken))[calisanId];

    public async Task<IReadOnlyDictionary<int, IReadOnlyList<Alici>>> OnaycilarAsync(
        IReadOnlyCollection<int> calisanIdler,
        CancellationToken cancellationToken)
    {
        var yoneticiler = await db.Calisanlar
            .Where(c => calisanIdler.Contains(c.Id))
            .Select(c => new { c.Id, c.Departman.YoneticiId })
            .ToDictionaryAsync(c => c.Id, c => c.YoneticiId == c.Id ? null : c.YoneticiId, cancellationToken);
        var yoneticiIdler = yoneticiler.Values.OfType<int>().Distinct().ToList();
        var yoneticiKullanicilari = (await AktifKullanicilar()
                .Where(k => k.CalisanId != null && yoneticiIdler.Contains(k.CalisanId.Value))
                .Select(k => new { YoneticiId = k.CalisanId!.Value, Alici = new Alici(k.Id, k.Email!) })
                .ToListAsync(cancellationToken))
            .ToLookup(k => k.YoneticiId, k => k.Alici);

        var onaycilar = yoneticiler.ToDictionary(
            c => c.Key,
            IReadOnlyList<Alici> (c) => c.Value is { } yoneticiId ? yoneticiKullanicilari[yoneticiId].ToList() : []);
        var ikyeDusenler = onaycilar.Where(o => o.Value.Count == 0).Select(o => o.Key).ToList();
        if (ikyeDusenler.Count == 0)
        {
            return onaycilar;
        }

        var tumSirketRolleri = db.RoleClaims
            .Where(c => c.ClaimType == Yetkiler.ClaimType && c.ClaimValue == Yetkiler.TumSirket)
            .Select(c => c.RoleId);
        var tumSirketKullanicilari = await AktifKullanicilar()
            .Where(k => db.UserRoles.Any(r => r.UserId == k.Id && tumSirketRolleri.Contains(r.RoleId)))
            .Select(k => new { k.CalisanId, Alici = new Alici(k.Id, k.Email!) })
            .ToListAsync(cancellationToken);
        foreach (var calisanId in ikyeDusenler)
        {
            onaycilar[calisanId] = tumSirketKullanicilari.Where(k => k.CalisanId != calisanId).Select(k => k.Alici).ToList();
        }

        return onaycilar;
    }

    public Task<IReadOnlyList<Alici>> CalisaninKendisiAsync(int calisanId, CancellationToken cancellationToken) =>
        Alicilar(AktifKullanicilar().Where(k => k.CalisanId == calisanId), cancellationToken);

    private IQueryable<Kullanici> AktifKullanicilar() => db.Users.Where(k => k.Aktif && k.Email != null);

    private static async Task<IReadOnlyList<Alici>> Alicilar(IQueryable<Kullanici> kullanicilar, CancellationToken cancellationToken) =>
        await kullanicilar.Select(k => new Alici(k.Id, k.Email!)).ToListAsync(cancellationToken);
}
