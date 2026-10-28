namespace IzinTakip.Api.Features.Roller;

public sealed record RolKaydetRequest(string Ad, bool Korumali, IReadOnlyList<string> Yetkiler);

public sealed record RolResponse(int Id, string Ad, bool Korumali, bool SistemRolu, IReadOnlyList<string> Yetkiler, int KullaniciSayisi);
