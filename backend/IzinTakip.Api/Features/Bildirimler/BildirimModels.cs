namespace IzinTakip.Api.Features.Bildirimler;

public sealed record BildirimListeRequest(bool SadeceOkunmamis = false, int Page = 1, int PageSize = 20);

public sealed record BildirimResponse(int Id, string Baslik, string Metin, string? Baglanti, bool OkunduMu, DateTime OlusturmaTarihi);

public sealed record BildirimSayisiResponse(int Okunmamis);
