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
    private const string HataliGiris = "E-posta ya da parola hatalı.";

    public async Task<GirisResponse> GirisAsync(GirisRequest request, CancellationToken cancellationToken)
    {
        var kullanici = await userManager.FindByEmailAsync(request.Eposta)
            ?? throw new KimlikDogrulanamadiException(HataliGiris);

        var sonuc = await signInManager.CheckPasswordSignInAsync(kullanici, request.Parola, lockoutOnFailure: true);
        if (sonuc.IsLockedOut)
        {
            throw new KimlikDogrulanamadiException("Çok sayıda hatalı deneme yapıldı. Birkaç dakika sonra tekrar deneyin.");
        }

        if (!sonuc.Succeeded)
        {
            throw new KimlikDogrulanamadiException(HataliGiris);
        }

        if (!kullanici.Aktif)
        {
            throw new KimlikDogrulanamadiException("Hesabınız pasif durumda. İK ile görüşün.");
        }

        var simdi = timeProvider.GetUtcNow().UtcDateTime;
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
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
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
        var bilgi = await gecerliKullanici.YetkileriAsync(cancellationToken);
        var kullanici = await userManager.FindByIdAsync(bilgi.KullaniciId.ToString(CultureInfo.InvariantCulture))
            ?? throw new KimlikDogrulanamadiException("Oturum açmanız gerekiyor.");

        kullanici.ParolaDegistirmeliMi = false;
        var sonuc = await userManager.ChangePasswordAsync(kullanici, request.MevcutParola, request.YeniParola);
        if (!sonuc.Succeeded)
        {
            throw new IsKuraliException(string.Join(" ", sonuc.Errors.Select(e => e.Description)));
        }

        yetkiOkuyucu.Unut(kullanici.Id);
        logger.LogInformation("Kullanici parolasini degistirdi {KullaniciId}", kullanici.Id);
    }
}
