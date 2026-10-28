using IzinTakip.Domain.Enums;

namespace IzinTakip.Api.Features.Raporlar;

public sealed record TakvimRequest(DateOnly Baslangic, DateOnly Bitis, int? DepartmanId);

public sealed record BakiyeRaporuRequest(int? DepartmanId, int Page = 1, int PageSize = 20);

public sealed record TakvimKaydiResponse(
    int IzinTalebiId,
    int CalisanId,
    string CalisanAdSoyad,
    string DepartmanAdi,
    string IzinTuruAdi,
    SureTipi SureTipi,
    DateTime Baslangic,
    DateTime Bitis,
    IzinDurumu Durum);

public sealed record BakiyeRaporuResponse(
    int CalisanId,
    string CalisanAdSoyad,
    string DepartmanAdi,
    DateOnly IseGirisTarihi,
    int HakDakika,
    string HakMetni,
    int KullanilanDakika,
    string KullanilanMetni,
    int BekleyenDakika,
    string BekleyenMetni,
    int KullanilabilirDakika,
    string KullanilabilirMetni);
