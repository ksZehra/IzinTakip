using System.Security.Claims;
using IzinTakip.Domain;
using IzinTakip.Domain.Entities;
using IzinTakip.Domain.Enums;
using IzinTakip.Domain.Izin;
using IzinTakip.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IzinTakip.Infrastructure.Data.Seed;

public sealed class BaslangicVerisi(
    IzinTakipDbContext db,
    RoleManager<Rol> roleManager,
    UserManager<Kullanici> userManager,
    ILogger<BaslangicVerisi> logger)
{
    public async Task HazirlaAsync(BaslangicVerisiAyarlari ayarlar, CancellationToken cancellationToken)
    {
        await RolEkleAsync(SistemRolleri.SuperAdmin, korumali: true, []);
        await RolEkleAsync(SistemRolleri.IK, korumali: true, Yetkiler.Atanabilir.Select(y => y.Kod).Where(kod => kod != Yetkiler.Denetim.Gor));
        await RolEkleAsync(SistemRolleri.Yonetici, korumali: false, [Yetkiler.IzinTalebi.Olustur, Yetkiler.IzinTalebi.Yonet, Yetkiler.Rapor.Gor]);
        await RolEkleAsync(SistemRolleri.Calisan, korumali: false, [Yetkiler.IzinTalebi.Olustur]);

        var gunluk = (await db.Ayarlar.AsNoTracking().SingleAsync(cancellationToken)).GunlukCalismaDakikasi;
        await IzinTurleriniEkleAsync(gunluk, cancellationToken);
        await HakKurallariniEkleAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(ayarlar.SuperAdminEposta) || string.IsNullOrWhiteSpace(ayarlar.SuperAdminParola))
        {
            logger.LogWarning("Seed:SuperAdmin ayarlari yok; SuperAdmin kullanicisi olusturulmadi");
        }
        else
        {
            await KullaniciEkleAsync(ayarlar.SuperAdminEposta, ayarlar.SuperAdminParola, SistemRolleri.SuperAdmin, calisanId: null);
        }

        if (ayarlar.DemoVeri)
        {
            await DemoVeriEkleAsync(ayarlar.DemoParola, gunluk, cancellationToken);
        }
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

    private async Task IzinTurleriniEkleAsync(int gunluk, CancellationToken cancellationToken)
    {
        if (await db.IzinTurleri.AnyAsync(cancellationToken))
        {
            return;
        }

        db.IzinTurleri.AddRange(
            new IzinTuru("Yıllık", true, false, GunHesabi.IsGunu, true, true, false, null, null),
            new IzinTuru("Saatlik İzin", true, false, GunHesabi.IsGunu, false, false, true, null, null),
            new IzinTuru("Evlilik", false, false, GunHesabi.IsGunu, true, false, false, 3 * gunluk, null),
            new IzinTuru("Ölüm", false, false, GunHesabi.IsGunu, true, false, false, 3 * gunluk, null),
            new IzinTuru("Babalık", false, false, GunHesabi.IsGunu, true, false, false, 5 * gunluk, null),
            new IzinTuru("Rapor", false, true, GunHesabi.TakvimGunu, true, false, false, null, null),
            new IzinTuru("Ücretsiz", false, false, GunHesabi.IsGunu, true, true, false, null, null));
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Varsayilan izin turleri eklendi");
    }

    private async Task HakKurallariniEkleAsync(CancellationToken cancellationToken)
    {
        if (await db.IzinHakkiKurallari.AnyAsync(cancellationToken))
        {
            return;
        }

        db.IzinHakkiKurallari.AddRange(
            new IzinHakkiKurali("1–5 yıl", 1, 5, null, null, 14),
            new IzinHakkiKurali("6–14 yıl", 6, 14, null, null, 20),
            new IzinHakkiKurali("15 yıl ve üstü", 15, null, null, null, 26),
            new IzinHakkiKurali("18 yaş ve altı", 1, null, null, 18, 20),
            new IzinHakkiKurali("50 yaş ve üstü", 1, null, 50, null, 20));
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Varsayilan hak kurallari eklendi");
    }

    private async Task DemoVeriEkleAsync(string? demoParola, int gunluk, CancellationToken cancellationToken)
    {
        if (await db.Departmanlar.AnyAsync(cancellationToken))
        {
            return;
        }

        var ik = new Departman("İnsan Kaynakları", null);
        var yazilim = new Departman("Yazılım Geliştirme", null);
        var satis = new Departman("Satış", null);
        db.Departmanlar.AddRange(ik, yazilim, satis);
        await db.SaveChangesAsync(cancellationToken);

        var acilis = new DateOnly(2026, 1, 1);
        Calisan CalisanOlustur(string ad, string soyad, int departmanId, DateOnly iseGiris, DateOnly dogum) =>
            new(ad, soyad, departmanId, iseGiris, dogum, IzinHakkiHesaplayici.IlkHakKidemYili(iseGiris, acilis));

        var ayse = CalisanOlustur("Ayşe", "Yılmaz", ik.Id, new DateOnly(2018, 3, 12), new DateOnly(1985, 6, 20));
        var mehmet = CalisanOlustur("Mehmet", "Kaya", yazilim.Id, new DateOnly(2015, 9, 1), new DateOnly(1980, 2, 14));
        var zeynep = CalisanOlustur("Zeynep", "Demir", yazilim.Id, new DateOnly(2022, 1, 10), new DateOnly(1996, 11, 3));
        var can = CalisanOlustur("Can", "Öztürk", yazilim.Id, new DateOnly(2024, 4, 15), new DateOnly(1999, 7, 22));
        var elif = CalisanOlustur("Elif", "Şahin", satis.Id, new DateOnly(2012, 5, 7), new DateOnly(1974, 9, 30));
        var burak = CalisanOlustur("Burak", "Arslan", satis.Id, new DateOnly(2025, 2, 3), new DateOnly(2003, 12, 12));
        Calisan[] calisanlar = [ayse, mehmet, zeynep, can, elif, burak];
        db.Calisanlar.AddRange(calisanlar);
        await db.SaveChangesAsync(cancellationToken);

        ik.Guncelle(ik.Ad, ayse.Id);
        yazilim.Guncelle(yazilim.Ad, mehmet.Id);
        satis.Guncelle(satis.Ad, elif.Id);
        db.IzinHaklari.AddRange(calisanlar.Select(c => IzinHakki.Elle(c.Id, acilis, 14 * gunluk, "Devreden bakiye")));
        db.ResmiTatiller.AddRange(new[] { 2026, 2027 }.SelectMany(SabitResmiTatiller));
        await db.SaveChangesAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(demoParola))
        {
            logger.LogWarning("Seed:Demo:Parola yok; demo kullanicilari olusturulmadi");
        }
        else
        {
            await KullaniciEkleAsync("ik@izintakip.local", demoParola, SistemRolleri.IK, ayse.Id);
            await KullaniciEkleAsync("yonetici@izintakip.local", demoParola, SistemRolleri.Yonetici, mehmet.Id);
            await KullaniciEkleAsync("calisan@izintakip.local", demoParola, SistemRolleri.Calisan, zeynep.Id);
        }

        logger.LogInformation("Demo verisi eklendi");
    }

    private static IEnumerable<ResmiTatil> SabitResmiTatiller(int yil) =>
    [
        new(new DateOnly(yil, 1, 1), "Yılbaşı", false),
        new(new DateOnly(yil, 4, 23), "Ulusal Egemenlik ve Çocuk Bayramı", false),
        new(new DateOnly(yil, 5, 1), "Emek ve Dayanışma Günü", false),
        new(new DateOnly(yil, 5, 19), "Atatürk'ü Anma, Gençlik ve Spor Bayramı", false),
        new(new DateOnly(yil, 7, 15), "Demokrasi ve Millî Birlik Günü", false),
        new(new DateOnly(yil, 8, 30), "Zafer Bayramı", false),
        new(new DateOnly(yil, 10, 28), "Cumhuriyet Bayramı arifesi", true),
        new(new DateOnly(yil, 10, 29), "Cumhuriyet Bayramı", false)
    ];

    private async Task KullaniciEkleAsync(string eposta, string parola, string rol, int? calisanId)
    {
        if (await userManager.FindByEmailAsync(eposta) is not null)
        {
            return;
        }

        var kullanici = new Kullanici { UserName = eposta, Email = eposta, EmailConfirmed = true, CalisanId = calisanId };
        Dogrula(await userManager.CreateAsync(kullanici, parola), $"{eposta} kullanıcısını oluşturma");
        Dogrula(await userManager.AddToRoleAsync(kullanici, rol), $"{eposta} kullanıcısına {rol} rolünü atama");
        logger.LogInformation("Kullanici olusturuldu {KullaniciId} {Rol}", kullanici.Id, rol);
    }

    private static void Dogrula(IdentityResult sonuc, string islem)
    {
        if (!sonuc.Succeeded)
        {
            throw new InvalidOperationException($"{islem} başarısız: {string.Join(" ", sonuc.Errors.Select(e => e.Description))}");
        }
    }
}
