using IzinTakip.Infrastructure.Audit;

namespace IzinTakip.Api.Features.IslemKayitlari;

public sealed record IslemKaydiListeRequest(
    int? KullaniciId,
    int? CalisanId,
    string? Tablo,
    DateOnly? Baslangic,
    DateOnly? Bitis,
    bool SadeceKendiKaydi = false,
    int Page = 1,
    int PageSize = 20);

public sealed record IslemKaydiResponse(
    int Id,
    DateTime Tarih,
    int? KullaniciId,
    string KullaniciAdi,
    string Islem,
    string Tablo,
    int? KayitId,
    int? IlgiliCalisanId,
    string? IlgiliCalisanAdi,
    int? IlgiliKullaniciId,
    bool KendiKaydi,
    IReadOnlyList<AlanDegisikligi> Degisiklikler);
