using System.Security.Claims;
using IzinTakip.Domain;
using IzinTakip.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace IzinTakip.Infrastructure.Data.Seed;

public sealed class BaslangicVerisi(
    RoleManager<Rol> roleManager,
    UserManager<Kullanici> userManager,
    ILogger<BaslangicVerisi> logger)
{
    public async Task HazirlaAsync(string? superAdminEposta, string? superAdminParola)
    {
        await RolEkleAsync(SistemRolleri.SuperAdmin, korumali: true, []);
        await RolEkleAsync(SistemRolleri.IK, korumali: true, Yetkiler.Atanabilir.Select(y => y.Kod));
        await RolEkleAsync(SistemRolleri.Yonetici, korumali: false, [Yetkiler.IzinTalebi.Olustur, Yetkiler.IzinTalebi.Yonet, Yetkiler.Rapor.Gor]);
        await RolEkleAsync(SistemRolleri.Calisan, korumali: false, [Yetkiler.IzinTalebi.Olustur]);

        if (string.IsNullOrWhiteSpace(superAdminEposta) || string.IsNullOrWhiteSpace(superAdminParola))
        {
            logger.LogWarning("Seed:SuperAdmin ayarlari yok; SuperAdmin kullanicisi olusturulmadi");
            return;
        }

        if (await userManager.FindByEmailAsync(superAdminEposta) is not null)
        {
            return;
        }

        var kullanici = new Kullanici { UserName = superAdminEposta, Email = superAdminEposta, EmailConfirmed = true };
        Dogrula(await userManager.CreateAsync(kullanici, superAdminParola), "SuperAdmin kullanıcısını oluşturma");
        Dogrula(await userManager.AddToRoleAsync(kullanici, SistemRolleri.SuperAdmin), "SuperAdmin rolünü atama");
        logger.LogInformation("SuperAdmin kullanicisi olusturuldu {KullaniciId}", kullanici.Id);
    }

    private async Task RolEkleAsync(string ad, bool korumali, IEnumerable<string> yetkiler)
    {
        if (await roleManager.RoleExistsAsync(ad))
        {
            return;
        }

        var rol = new Rol(ad) { Korumali = korumali };
        Dogrula(await roleManager.CreateAsync(rol), $"{ad} rolünü oluşturma");
        foreach (var yetki in yetkiler)
        {
            Dogrula(await roleManager.AddClaimAsync(rol, new Claim(Yetkiler.ClaimType, yetki)), $"{ad} rolüne {yetki} yetkisini ekleme");
        }

        logger.LogInformation("Rol olusturuldu {Rol}", ad);
    }

    private static void Dogrula(IdentityResult sonuc, string islem)
    {
        if (!sonuc.Succeeded)
        {
            throw new InvalidOperationException($"{islem} başarısız: {string.Join(" ", sonuc.Errors.Select(e => e.Description))}");
        }
    }
}
