using System.Security.Cryptography;
using IzinTakip.Api.Common;
using IzinTakip.Api.Common.Auth;
using IzinTakip.Api.Common.Paging;
using IzinTakip.Domain;
using IzinTakip.Domain.Exceptions;
using IzinTakip.Infrastructure.Data;
using IzinTakip.Infrastructure.Email;
using IzinTakip.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static IzinTakip.Api.Common.Auth.IdentitySonucu;

namespace IzinTakip.Api.Features.Kullanicilar;

public sealed class KullaniciService(
    IzinTakipDbContext db,
    UserManager<Kullanici> userManager,
    IGecerliKullanici gecerliKullanici,
    KullaniciYetkiOkuyucu yetkiOkuyucu,
    KullaniciKorumasi koruma,
    IEmailQueue emailQueue,
    UygulamaAyarlari uygulama,
    TimeProvider timeProvider,
    ILogger<KullaniciService> logger)
{
    private const int GeciciParolaGun = 3;

    public async Task<PagedResponse<KullaniciResponse>> ListeleAsync(KullaniciListeRequest request, CancellationToken cancellationToken)
    {
        var sorgu = db.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(request.Arama))
        {
            var arama = request.Arama.Trim();
            sorgu = sorgu.Where(k => k.Email!.Contains(arama)
                                     || db.Calisanlar.Any(c => c.Id == k.CalisanId && (c.Ad + " " + c.Soyad).Contains(arama)));
        }

        if (request.Aktif is not null)
        {
            sorgu = sorgu.Where(k => k.Aktif == request.Aktif);
        }

        return await Yansit(sorgu.OrderBy(k => k.Email).ThenBy(k => k.Id)).SayfalaAsync(request.Page, request.PageSize, cancellationToken);
    }

    public async Task<KullaniciResponse> DetayAsync(int id, CancellationToken cancellationToken) =>
        await Yansit(db.Users.AsNoTracking().Where(k => k.Id == id)).SingleOrDefaultAsync(cancellationToken)
        ?? throw new KayitBulunamadiException("Kullanıcı bulunamadı.");

    public async Task<KullaniciResponse> OlusturAsync(KullaniciOlusturRequest request, CancellationToken cancellationToken)
    {
        var yapan = await gecerliKullanici.YetkileriAsync(cancellationToken);
        var eposta = request.Eposta.Trim();
        await RolleriDenetleAsync(request.Roller, yapan, cancellationToken);
        if (await userManager.FindByEmailAsync(eposta) is not null)
        {
            throw new IsKuraliException("Bu e-posta adresi başka bir kullanıcıda kayıtlı.");
        }

        if (request.CalisanId is { } calisanId)
        {
            if (!await db.Calisanlar.AnyAsync(c => c.Id == calisanId && c.Aktif, cancellationToken))
            {
                throw new IsKuraliException("Seçilen çalışan bulunamadı ya da pasif.");
            }

            if (await db.Users.AnyAsync(k => k.CalisanId == calisanId, cancellationToken))
            {
                throw new IsKuraliException("Bu çalışanın zaten bir kullanıcı hesabı var.");
            }
        }

        var parola = GeciciParola();
        var kullanici = new Kullanici
        {
            UserName = eposta,
            Email = eposta,
            CalisanId = request.CalisanId,
            ParolaDegistirmeliMi = true,
            GeciciParolaBitis = GeciciParolaBitisi()
        };
        await using (var transaction = await db.Database.BeginTransactionAsync(cancellationToken))
        {
            Dogrula(await userManager.CreateAsync(kullanici, parola));
            Dogrula(await userManager.AddToRolesAsync(kullanici, request.Roller.Distinct(StringComparer.OrdinalIgnoreCase)));
            await transaction.CommitAsync(cancellationToken);
        }

        ParolaGonder(eposta, "İzin Takip hesabınız açıldı", parola);
        logger.LogInformation("Kullanici olusturuldu {KullaniciId} {Roller} {YapanKullaniciId}", kullanici.Id, request.Roller, yapan.KullaniciId);
        return await DetayAsync(kullanici.Id, cancellationToken);
    }

    public async Task<KullaniciResponse> RolleriDegistirAsync(int id, KullaniciRolleriRequest request, CancellationToken cancellationToken)
    {
        var yapan = await gecerliKullanici.YetkileriAsync(cancellationToken);
        var kullanici = await BulAsync(id, cancellationToken);
        await koruma.DokunabilirMiAsync(id, yapan, cancellationToken);
        await RolleriDenetleAsync(request.Roller, yapan, cancellationToken);

        var mevcut = await userManager.GetRolesAsync(kullanici);
        var yeni = request.Roller.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (mevcut.Contains(SistemRolleri.SuperAdmin)
            && !yeni.Contains(SistemRolleri.SuperAdmin, StringComparer.OrdinalIgnoreCase)
            && await koruma.SonAktifSuperAdminMiAsync(id, cancellationToken))
        {
            throw new IsKuraliException("Son aktif SuperAdmin'in SuperAdmin rolü kaldırılamaz.");
        }

        await using (var transaction = await db.Database.BeginTransactionAsync(cancellationToken))
        {
            Dogrula(await userManager.RemoveFromRolesAsync(kullanici, mevcut.Except(yeni, StringComparer.OrdinalIgnoreCase)));
            Dogrula(await userManager.AddToRolesAsync(kullanici, yeni.Except(mevcut, StringComparer.OrdinalIgnoreCase)));
            await transaction.CommitAsync(cancellationToken);
        }

        yetkiOkuyucu.Unut(id);
        logger.LogInformation(
            "Kullanici rolleri degisti {KullaniciId} {EskiRoller} {YeniRoller} {YapanKullaniciId}", id, mevcut, yeni, yapan.KullaniciId);
        return await DetayAsync(id, cancellationToken);
    }

    public async Task<KullaniciResponse> AktiflikDegistirAsync(int id, bool aktif, CancellationToken cancellationToken)
    {
        var yapan = await gecerliKullanici.YetkileriAsync(cancellationToken);
        var kullanici = await BulAsync(id, cancellationToken);
        await koruma.DokunabilirMiAsync(id, yapan, cancellationToken);
        if (!aktif && id == yapan.KullaniciId)
        {
            throw new IsKuraliException("Kendi hesabınızı pasifleştiremezsiniz.");
        }

        if (!aktif && await koruma.SonAktifSuperAdminMiAsync(id, cancellationToken))
        {
            throw new IsKuraliException("Son aktif SuperAdmin pasifleştirilemez.");
        }

        if (aktif && kullanici.CalisanId is { } calisanId && !await db.Calisanlar.AnyAsync(c => c.Id == calisanId && c.Aktif, cancellationToken))
        {
            throw new IsKuraliException("Bağlı çalışan kaydı pasif; önce çalışanı aktifleştirin.");
        }

        kullanici.Aktif = aktif;
        await db.SaveChangesAsync(cancellationToken);

        yetkiOkuyucu.Unut(id);
        logger.LogInformation("Kullanici aktifligi degisti {KullaniciId} {Aktif} {YapanKullaniciId}", id, aktif, yapan.KullaniciId);
        return await DetayAsync(id, cancellationToken);
    }

    public async Task ParolaSifirlaAsync(int id, CancellationToken cancellationToken)
    {
        var yapan = await gecerliKullanici.YetkileriAsync(cancellationToken);
        var kullanici = await BulAsync(id, cancellationToken);
        await koruma.DokunabilirMiAsync(id, yapan, cancellationToken);

        var parola = GeciciParola();
        await using (var transaction = await db.Database.BeginTransactionAsync(cancellationToken))
        {
            Dogrula(await userManager.RemovePasswordAsync(kullanici));
            Dogrula(await userManager.AddPasswordAsync(kullanici, parola));
            kullanici.ParolaDegistirmeliMi = true;
            kullanici.GeciciParolaBitis = GeciciParolaBitisi();
            kullanici.LockoutEnd = null;
            kullanici.AccessFailedCount = 0;
            Dogrula(await userManager.UpdateAsync(kullanici));
            await transaction.CommitAsync(cancellationToken);
        }

        yetkiOkuyucu.Unut(id);
        ParolaGonder(kullanici.Email!, "İzin Takip parolanız sıfırlandı", parola);
        logger.LogInformation("Kullanici parolasi sifirlandi {KullaniciId} {YapanKullaniciId}", id, yapan.KullaniciId);
    }

    private async Task RolleriDenetleAsync(IReadOnlyList<string> roller, KullaniciYetkileri yapan, CancellationToken cancellationToken)
    {
        var bulunanlar = await db.Roles
            .AsNoTracking()
            .Where(r => roller.Contains(r.Name!))
            .Select(r => r.Korumali)
            .ToListAsync(cancellationToken);
        if (bulunanlar.Count != roller.Distinct(StringComparer.OrdinalIgnoreCase).Count())
        {
            throw new IsKuraliException("Seçilen rollerden biri bulunamadı.");
        }

        if (!yapan.SuperAdmin && bulunanlar.Contains(true))
        {
            throw new YetkiYokException("Korumalı rolü yalnız SuperAdmin atayabilir.");
        }
    }

    private async Task<Kullanici> BulAsync(int id, CancellationToken cancellationToken) =>
        await db.Users.SingleOrDefaultAsync(k => k.Id == id, cancellationToken)
        ?? throw new KayitBulunamadiException("Kullanıcı bulunamadı.");

    private IQueryable<KullaniciResponse> Yansit(IQueryable<Kullanici> kullanicilar) =>
        kullanicilar.Select(k => new KullaniciResponse(
            k.Id,
            k.Email!,
            k.CalisanId,
            db.Calisanlar.Where(c => c.Id == k.CalisanId).Select(c => c.Ad + " " + c.Soyad).FirstOrDefault(),
            k.Aktif,
            k.ParolaDegistirmeliMi,
            db.UserRoles
                .Where(ur => ur.UserId == k.Id)
                .Join(db.Roles, ur => ur.RoleId, r => r.Id, (_, r) => r.Name!)
                .OrderBy(ad => ad)
                .ToList()));

    private void ParolaGonder(string eposta, string konu, string parola) =>
        emailQueue.Enqueue(new EmailMesaji(
            eposta,
            konu,
            EmailSablonu.Bildirim(
                konu,
                $"Giriş e-postanız: {eposta}. Geçici parolanız: {parola}. Parola {GeciciParolaGun} gün geçerlidir; ilk girişte yeni bir parola belirlemeniz istenecek.",
                uygulama.Adres.TrimEnd('/') + "/giris",
                "Giriş yap")));

    private DateTime GeciciParolaBitisi() => timeProvider.GetUtcNow().UtcDateTime.AddDays(GeciciParolaGun);

    private static string GeciciParola()
    {
        const string buyuk = "ABCDEFGHJKLMNPRSTUVYZ";
        const string kucuk = "abcdefghijkmnoprstuvyz";
        const string rakam = "23456789";
        char[] parola =
        [
            .. RandomNumberGenerator.GetItems<char>(buyuk, 1),
            .. RandomNumberGenerator.GetItems<char>(kucuk, 1),
            .. RandomNumberGenerator.GetItems<char>(rakam, 1),
            .. RandomNumberGenerator.GetItems<char>(buyuk + kucuk + rakam, 9)
        ];
        RandomNumberGenerator.Shuffle(parola.AsSpan());
        return new string(parola);
    }
}
