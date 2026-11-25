namespace IzinTakip.Api.Features.Kullanicilar;

public sealed record KullaniciListeRequest(string? Arama, bool? Aktif, string[]? Rol, string? Sort, int Page = 1, int PageSize = 20);

public sealed record KullaniciOlusturRequest(string Eposta, int? CalisanId, IReadOnlyList<string> Roller);

public sealed record KullaniciRolleriRequest(IReadOnlyList<string> Roller);

public sealed record KullaniciResponse(
    int Id,
    string Eposta,
    int? CalisanId,
    string? CalisanAdSoyad,
    bool Aktif,
    bool ParolaDegistirmeli,
    IReadOnlyList<string> Roller);
