using IzinTakip.Domain.Enums;

namespace IzinTakip.Api.Features.Calisanlar;

public sealed record CalisanListeRequest(
    string? Arama,
    int[]? DepartmanId,
    bool? Aktif,
    DateOnly? IseGirisBaslangic,
    DateOnly? IseGirisBitis,
    string? Sort,
    int Page = 1,
    int PageSize = 20);

public sealed record CalisanKaydetRequest(
    string Ad,
    string Soyad,
    int DepartmanId,
    DateOnly? IseGirisTarihi,
    DateOnly? DogumTarihi,
    int? HakBaslangicKidemYili);

public sealed record IzinHakkiKaydetRequest(int Dakika, string Aciklama, DateOnly? KazanimTarihi);

public sealed record CalisanListeResponse(
    int Id,
    string Ad,
    string Soyad,
    int DepartmanId,
    string DepartmanAdi,
    DateOnly IseGirisTarihi,
    bool Aktif);

public sealed record CalisanResponse(
    int Id,
    string Ad,
    string Soyad,
    int DepartmanId,
    string DepartmanAdi,
    DateOnly IseGirisTarihi,
    DateOnly DogumTarihi,
    int HakBaslangicKidemYili,
    bool Aktif,
    string? KullaniciEposta);

public sealed record IzinHakkiResponse(
    int Id,
    DateOnly KazanimTarihi,
    int Dakika,
    string SureMetni,
    HakKaynagi Kaynak,
    int? KidemYili,
    string Aciklama);

public sealed record BakiyeResponse(
    int HakDakika,
    string HakMetni,
    int KullanilanDakika,
    string KullanilanMetni,
    int BekleyenDakika,
    string BekleyenMetni,
    int KullanilabilirDakika,
    string KullanilabilirMetni);

public sealed record IzinHaklariResponse(IReadOnlyList<IzinHakkiResponse> Satirlar, int ToplamDakika, string ToplamSureMetni);
