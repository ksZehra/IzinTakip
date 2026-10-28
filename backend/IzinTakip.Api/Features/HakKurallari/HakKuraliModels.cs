namespace IzinTakip.Api.Features.HakKurallari;

public sealed record HakKuraliKaydetRequest(string Ad, int MinKidemYil, int? MaxKidemYil, int? MinYas, int? MaxYas, int Gun);

public sealed record HakKuraliResponse(int Id, string Ad, int MinKidemYil, int? MaxKidemYil, int? MinYas, int? MaxYas, int Gun, bool Aktif);
