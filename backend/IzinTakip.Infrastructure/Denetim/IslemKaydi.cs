using IzinTakip.Domain.Entities;
using IzinTakip.Infrastructure.Identity;

namespace IzinTakip.Infrastructure.Denetim;

public sealed class IslemKaydi
{
    public int Id { get; init; }
    public DateTime Tarih { get; init; }
    public int? KullaniciId { get; init; }
    public string Islem { get; init; } = string.Empty;
    public string Tablo { get; init; } = string.Empty;
    public int? KayitId { get; init; }
    public int? IlgiliCalisanId { get; init; }
    public Calisan? IlgiliCalisan { get; init; }
    public int? IlgiliKullaniciId { get; init; }
    public Kullanici? IlgiliKullanici { get; init; }
    public string Degisiklikler { get; init; } = string.Empty;
}

public sealed record AlanDegisikligi(string Alan, string? Eski, string? Yeni);
