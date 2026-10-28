namespace IzinTakip.Api.Features.Auth;

public sealed record GirisRequest(string Eposta, string Parola);

public sealed record GirisResponse(string AccessToken, DateTime SureBitis, bool ParolaDegistirmeli);

public sealed record BenResponse(
    int KullaniciId,
    string Eposta,
    int? CalisanId,
    IReadOnlyList<string> Roller,
    IReadOnlyList<string> Yetkiler,
    bool ParolaDegistirmeli);

public sealed record ParolaDegistirRequest(string MevcutParola, string YeniParola);
