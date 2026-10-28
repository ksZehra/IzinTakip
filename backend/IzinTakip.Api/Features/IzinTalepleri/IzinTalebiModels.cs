using IzinTakip.Domain.Enums;

namespace IzinTakip.Api.Features.IzinTalepleri;

public record SureHesaplaRequest(
    int IzinTuruId,
    SureTipi SureTipi,
    DateOnly Tarih,
    DateOnly? BitisTarihi,
    GunYarisi? GunYarisi,
    TimeOnly? BaslangicSaati,
    TimeOnly? BitisSaati);

public sealed record IzinTalebiOlusturRequest(
    int? CalisanId,
    int IzinTuruId,
    SureTipi SureTipi,
    DateOnly Tarih,
    DateOnly? BitisTarihi,
    GunYarisi? GunYarisi,
    TimeOnly? BaslangicSaati,
    TimeOnly? BitisSaati,
    string? Aciklama)
    : SureHesaplaRequest(IzinTuruId, SureTipi, Tarih, BitisTarihi, GunYarisi, BaslangicSaati, BitisSaati);

public sealed record IzinTalebiListeRequest(
    int? CalisanId,
    IzinDurumu? Durum,
    int? IzinTuruId,
    DateOnly? Baslangic,
    DateOnly? Bitis,
    int Page = 1,
    int PageSize = 20);

public sealed record SureHesaplaResponse(int Dakika, string SureMetni, DateTime Baslangic, DateTime Bitis);

public sealed record IzinTalebiListeResponse(
    int Id,
    int CalisanId,
    string CalisanAdSoyad,
    string IzinTuruAdi,
    SureTipi SureTipi,
    DateTime Baslangic,
    DateTime Bitis,
    int Dakika,
    string SureMetni,
    IzinDurumu Durum);

public sealed record IzinTalebiResponse(
    int Id,
    int CalisanId,
    string CalisanAdSoyad,
    string DepartmanAdi,
    int IzinTuruId,
    string IzinTuruAdi,
    SureTipi SureTipi,
    DateTime Baslangic,
    DateTime Bitis,
    int Dakika,
    string SureMetni,
    IzinDurumu Durum,
    string? Aciklama,
    DateTime OlusturmaTarihi,
    int? KararVerenKullaniciId,
    DateTime? KararTarihi,
    string? KararNotu);
