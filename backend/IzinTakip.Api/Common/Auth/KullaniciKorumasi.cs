using IzinTakip.Domain;
using IzinTakip.Domain.Exceptions;
using IzinTakip.Infrastructure.Data;
using IzinTakip.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace IzinTakip.Api.Common.Auth;

public sealed class KullaniciKorumasi(IzinTakipDbContext db)
{
    public async Task DokunabilirMiAsync(int kullaniciId, KullaniciYetkileri yapan, CancellationToken cancellationToken)
    {
        if (!yapan.SuperAdmin
            && await db.Roles.AnyAsync(r => r.Korumali && db.UserRoles.Any(ur => ur.UserId == kullaniciId && ur.RoleId == r.Id), cancellationToken))
        {
            throw new ForbiddenException("Korumalı role sahip kullanıcıda değişiklik yapma yetkiniz yok.");
        }
    }

    public async Task<bool> SonAktifSuperAdminMiAsync(int kullaniciId, CancellationToken cancellationToken)
    {
        var superAdminRolu = db.Roles.Where(r => r.Name == SistemRolleri.SuperAdmin).Select(r => r.Id);
        var aktifSuperAdminler = db.Users.Where(k =>
            k.Aktif && db.UserRoles.Any(ur => ur.UserId == k.Id && superAdminRolu.Contains(ur.RoleId)));

        return await aktifSuperAdminler.AnyAsync(k => k.Id == kullaniciId, cancellationToken)
               && !await aktifSuperAdminler.AnyAsync(k => k.Id != kullaniciId, cancellationToken);
    }
}
