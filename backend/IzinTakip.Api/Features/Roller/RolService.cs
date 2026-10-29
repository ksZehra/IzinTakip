using System.Globalization;
using System.Security.Claims;
using IzinTakip.Domain;
using IzinTakip.Domain.Exceptions;
using IzinTakip.Infrastructure.Data;
using IzinTakip.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static IzinTakip.Api.Common.Auth.IdentitySonucu;

namespace IzinTakip.Api.Features.Roller;

public sealed class RolService(
    IzinTakipDbContext db,
    RoleManager<Rol> roleManager,
    IGecerliKullanici gecerliKullanici,
    KullaniciYetkiOkuyucu yetkiOkuyucu,
    ILogger<RolService> logger)
{
    public async Task<IReadOnlyList<RolResponse>> ListeleAsync(CancellationToken cancellationToken) =>
        await Yansit(db.Roles.AsNoTracking().OrderBy(r => r.Name)).ToListAsync(cancellationToken);

    public async Task<RolResponse> DetayAsync(int id, CancellationToken cancellationToken) =>
        await Yansit(db.Roles.AsNoTracking().Where(r => r.Id == id)).SingleOrDefaultAsync(cancellationToken)
        ?? throw new KayitBulunamadiException("Rol bulunamadı.");

    public async Task<RolResponse> OlusturAsync(RolKaydetRequest request, CancellationToken cancellationToken)
    {
        var yapan = await gecerliKullanici.YetkileriAsync(cancellationToken);
        var rol = new Rol(request.Ad.Trim()) { Korumali = request.Korumali };
        var yetkiler = request.Yetkiler.Distinct().ToList();

        await using (var transaction = await db.Database.BeginTransactionAsync(cancellationToken))
        {
            Dogrula(await roleManager.CreateAsync(rol));
            foreach (var yetki in yetkiler)
            {
                Dogrula(await roleManager.AddClaimAsync(rol, new Claim(Yetkiler.ClaimType, yetki)));
            }

            await transaction.CommitAsync(cancellationToken);
        }

        logger.LogInformation(
            "Rol olusturuldu {RolId} {Ad} {Korumali} {Yetkiler} {YapanKullaniciId}", rol.Id, rol.Name, rol.Korumali, yetkiler, yapan.KullaniciId);
        return await DetayAsync(rol.Id, cancellationToken);
    }

    public async Task<RolResponse> GuncelleAsync(int id, RolKaydetRequest request, CancellationToken cancellationToken)
    {
        var yapan = await gecerliKullanici.YetkileriAsync(cancellationToken);
        var rol = await BulAsync(id);
        if (rol.Name == SistemRolleri.SuperAdmin)
        {
            throw new IsKuraliException("SuperAdmin rolü değiştirilemez.");
        }

        var ad = request.Ad.Trim();
        if (SistemRolleri.Tumu.Contains(rol.Name) && ad != rol.Name)
        {
            throw new IsKuraliException("Sistem rollerinin adı değiştirilemez.");
        }

        var yeniYetkiler = request.Yetkiler.Distinct().ToList();
        List<string> eskiYetkiler;
        await using (var transaction = await db.Database.BeginTransactionAsync(cancellationToken))
        {
            rol.Name = ad;
            rol.Korumali = request.Korumali;
            Dogrula(await roleManager.UpdateAsync(rol));

            eskiYetkiler = (await roleManager.GetClaimsAsync(rol)).Where(c => c.Type == Yetkiler.ClaimType).Select(c => c.Value).ToList();
            foreach (var yetki in eskiYetkiler.Except(yeniYetkiler))
            {
                Dogrula(await roleManager.RemoveClaimAsync(rol, new Claim(Yetkiler.ClaimType, yetki)));
            }

            foreach (var yetki in yeniYetkiler.Except(eskiYetkiler))
            {
                Dogrula(await roleManager.AddClaimAsync(rol, new Claim(Yetkiler.ClaimType, yetki)));
            }

            await transaction.CommitAsync(cancellationToken);
        }

        foreach (var kullaniciId in await db.UserRoles.Where(ur => ur.RoleId == id).Select(ur => ur.UserId).ToListAsync(cancellationToken))
        {
            yetkiOkuyucu.Unut(kullaniciId);
        }

        logger.LogInformation(
            "Rol guncellendi {RolId} {Ad} {Korumali} {EskiYetkiler} {YeniYetkiler} {YapanKullaniciId}",
            id, ad, request.Korumali, eskiYetkiler, yeniYetkiler, yapan.KullaniciId);
        return await DetayAsync(id, cancellationToken);
    }

    public async Task SilAsync(int id, CancellationToken cancellationToken)
    {
        var yapan = await gecerliKullanici.YetkileriAsync(cancellationToken);
        var rol = await BulAsync(id);
        if (SistemRolleri.Tumu.Contains(rol.Name))
        {
            throw new IsKuraliException("Sistem rolleri silinemez.");
        }

        if (await db.UserRoles.AnyAsync(ur => ur.RoleId == id, cancellationToken))
        {
            throw new IsKuraliException("Bu role atanmış kullanıcılar var; önce kullanıcıların rollerini değiştirin.");
        }

        Dogrula(await roleManager.DeleteAsync(rol));
        logger.LogInformation("Rol silindi {RolId} {Ad} {YapanKullaniciId}", id, rol.Name, yapan.KullaniciId);
    }

    private async Task<Rol> BulAsync(int id) =>
        await roleManager.FindByIdAsync(id.ToString(CultureInfo.InvariantCulture))
        ?? throw new KayitBulunamadiException("Rol bulunamadı.");

    private IQueryable<RolResponse> Yansit(IQueryable<Rol> roller) =>
        roller.Select(r => new RolResponse(
            r.Id,
            r.Name!,
            r.Korumali,
            SistemRolleri.Tumu.Contains(r.Name!),
            db.RoleClaims.Where(c => c.RoleId == r.Id && c.ClaimType == Yetkiler.ClaimType).Select(c => c.ClaimValue!).OrderBy(k => k).ToList(),
            db.UserRoles.Count(ur => ur.RoleId == r.Id)));
}
