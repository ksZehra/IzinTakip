using System.Globalization;
using System.Security.Claims;
using IzinTakip.Api.Common;
using IzinTakip.Api.Common.Auth;
using IzinTakip.Domain;
using IzinTakip.Domain.Exceptions;
using IzinTakip.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace IzinTakip.Api.Features.Auth;

public sealed class AuthService(
    UserManager<Kullanici> userManager,
    SignInManager<Kullanici> signInManager,
    IGecerliKullanici gecerliKullanici,
    KullaniciYetkiOkuyucu yetkiOkuyucu,
    KullaniciAdlari kullaniciAdlari,
    JwtAyarlari jwtAyarlari,
    TimeProvider timeProvider,
    ILogger<AuthService> logger)
{
    private const string HataliGiris = "E-posta ya da parola hatalı. Üst üste hatalı denemede hesap birkaç dakika kilitlenir.";

    private static readonly string SahteParolaOzeti = new PasswordHasher<Kullanici>().HashPassword(new Kullanici(), Guid.NewGuid().ToString());

    public async Task<GirisResponse> GirisAsync(GirisRequest request)
    {
        var kullanici = await userManager.FindByEmailAsync(request.Eposta);
        if (kullanici is null)
        {
            userManager.PasswordHasher.VerifyHashedPassword(new Kullanici(), SahteParolaOzeti, request.Parola);
            logger.LogWarning("Basarisiz giris: kullanici bulunamadi");
            throw new KimlikDogrulanamadiException(HataliGiris);
        }

        var sonuc = await signInManager.CheckPasswordSignInAsync(kullanici, request.Parola, lockoutOnFailure: true);
        if (!sonuc.Succeeded)
        {
            if (sonuc.IsLockedOut)
            {
                userManager.PasswordHasher.VerifyHashedPassword(kullanici, SahteParolaOzeti, request.Parola);
            }

            logger.LogWarning("Basarisiz giris {KullaniciId} {Kilitli}", kullanici.Id, sonuc.IsLockedOut);
            throw new KimlikDogrulanamadiException(HataliGiris);
        }

        if (!kullanici.Aktif)
        {
            throw new KimlikDogrulanamadiException("Hesabınız pasif durumda. İK ile görüşün.");
        }

        var simdi = timeProvider.GetUtcNow().UtcDateTime;
        if (kullanici.GeciciParolaBitis <= simdi)
        {
            throw new KimlikDogrulanamadiException("Geçici parolanızın süresi doldu. Parolanızı yeniden sıfırlaması için İK ile görüşün.");
        }

        var sureBitis = simdi.AddHours(jwtAyarlari.SureSaat);
        var token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = jwtAyarlari.Issuer,
            Audience = jwtAyarlari.Audience,
            IssuedAt = simdi,
            NotBefore = simdi,
            Expires = sureBitis,
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, kullanici.Id.ToString(CultureInfo.InvariantCulture)),
                new Claim(JwtRegisteredClaimNames.Email, kullanici.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtAyarlari.GuvenlikDamgasiClaim, await userManager.GetSecurityStampAsync(kullanici))
            ]),
            SigningCredentials = new SigningCredentials(jwtAyarlari.ImzaAnahtari(), SecurityAlgorithms.HmacSha256)
        });

        logger.LogInformation("Kullanici giris yapti {KullaniciId}", kullanici.Id);
        return new GirisResponse(token, sureBitis, kullanici.ParolaDegistirmeliMi);
    }

    public async Task<BenResponse> BenAsync(CancellationToken cancellationToken)
    {
        var bilgi = await gecerliKullanici.YetkileriAsync(cancellationToken);
        var yetkiler = bilgi.SuperAdmin
            ? Yetkiler.Atanabilir.Select(y => y.Kod).Append(Yetkiler.RolYonetimi).ToList()
            : bilgi.Yetkiler.Order().ToList();

        var gorunenAd = (await kullaniciAdlari.OkuAsync([bilgi.KullaniciId], cancellationToken))[bilgi.KullaniciId];
        return new BenResponse(bilgi.KullaniciId, bilgi.Eposta, gorunenAd, bilgi.CalisanId, bilgi.Roller, yetkiler, bilgi.ParolaDegistirmeli);
    }

    public async Task ParolaDegistirAsync(ParolaDegistirRequest request, CancellationToken cancellationToken)
    {
        var kullanici = await OturumdakiKullaniciAsync(cancellationToken);
        var kontrol = await signInManager.CheckPasswordSignInAsync(kullanici, request.MevcutParola, lockoutOnFailure: true);
        if (!kontrol.Succeeded)
        {
            logger.LogWarning("Parola degistirmede hatali mevcut parola {KullaniciId} {Kilitli}", kullanici.Id, kontrol.IsLockedOut);
            throw new IsKuraliException(kontrol.IsLockedOut
                ? "Çok fazla hatalı deneme yapıldı. Birkaç dakika sonra tekrar deneyin."
                : "Mevcut parola hatalı. Üst üste hatalı denemede hesap birkaç dakika kilitlenir.");
        }

        kullanici.ParolaDegistirmeliMi = false;
        kullanici.GeciciParolaBitis = null;
        IdentitySonucu.Dogrula(await userManager.ChangePasswordAsync(kullanici, request.MevcutParola, request.YeniParola));

        yetkiOkuyucu.Unut(kullanici.Id);
        logger.LogInformation("Kullanici parolasini degistirdi {KullaniciId}", kullanici.Id);
    }

    public async Task CikisAsync(CancellationToken cancellationToken)
    {
        var kullanici = await OturumdakiKullaniciAsync(cancellationToken);
        await userManager.UpdateSecurityStampAsync(kullanici);

        yetkiOkuyucu.Unut(kullanici.Id);
        logger.LogInformation("Kullanici cikis yapti, oturumlari kapatildi {KullaniciId}", kullanici.Id);
    }

    private async Task<Kullanici> OturumdakiKullaniciAsync(CancellationToken cancellationToken)
    {
        var bilgi = await gecerliKullanici.YetkileriAsync(cancellationToken);
        return await userManager.FindByIdAsync(bilgi.KullaniciId.ToString(CultureInfo.InvariantCulture))
            ?? throw new KimlikDogrulanamadiException("Oturum açmanız gerekiyor.");
    }
}
