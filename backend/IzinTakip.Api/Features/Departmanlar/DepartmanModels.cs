namespace IzinTakip.Api.Features.Departmanlar;

public sealed record DepartmanKaydetRequest(string Ad, int? YoneticiId);

public sealed record DepartmanResponse(
    int Id,
    string Ad,
    int? YoneticiId,
    string? YoneticiAdSoyad,
    bool Aktif,
    int AktifCalisanSayisi);
