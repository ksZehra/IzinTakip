using System.Globalization;
using System.Text.Json;
using IzinTakip.Api.Common;
using IzinTakip.Api.Common.Izin;
using IzinTakip.Api.Common.Paging;
using IzinTakip.Domain;
using IzinTakip.Infrastructure.Audit;
using IzinTakip.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IzinTakip.Api.Features.IslemKayitlari;

public sealed class IslemKaydiService(IzinTakipDbContext db, KullaniciAdlariProvider kullaniciAdlari, SirketSaati sirketSaati)
{
    private static readonly HashSet<string> SureAlanlari = ["Dakika", "TalepBasinaEnFazlaDakika", "YillikEnFazlaDakika"];
    private static readonly HashSet<string> CalisanAlanlari = ["CalisanId", "YoneticiId"];
    private static readonly HashSet<string> DepartmanAlanlari = ["DepartmanId"];
    private static readonly HashSet<string> KullaniciAlanlari = ["UserId"];

    public static readonly IReadOnlyDictionary<string, Func<IQueryable<IslemKaydi>, bool, IOrderedQueryable<IslemKaydi>>> SiralamaAlanlari =
        new Dictionary<string, Func<IQueryable<IslemKaydi>, bool, IOrderedQueryable<IslemKaydi>>>
        {
            ["tarih"] = SiralamaExtensions.SiralamaAlani<IslemKaydi, DateTime>(k => k.Tarih),
            ["islem"] = SiralamaExtensions.SiralamaAlani<IslemKaydi, string>(k => k.Islem),
            ["tablo"] = SiralamaExtensions.SiralamaAlani<IslemKaydi, string>(k => k.Tablo),
            ["ilgiliCalisanAdi"] = (sorgu, azalan) => azalan
                ? sorgu.OrderByDescending(k => k.IlgiliCalisan!.AdSoyad)
                : sorgu.OrderBy(k => k.IlgiliCalisan!.AdSoyad)
        };

    public async Task<PagedResponse<IslemKaydiResponse>> ListeleAsync(IslemKaydiListeRequest request, CancellationToken cancellationToken)
    {
        var kayitlar = db.IslemKayitlari.AsNoTracking();
        if (request.KullaniciId is not null)
        {
            kayitlar = kayitlar.Where(k => k.KullaniciId == request.KullaniciId);
        }

        if (request.CalisanId is not null)
        {
            kayitlar = kayitlar.Where(k => k.IlgiliCalisanId == request.CalisanId);
        }

        if (!string.IsNullOrWhiteSpace(request.IlgiliArama))
        {
            var arama = request.IlgiliArama.Trim();
            kayitlar = kayitlar.Where(k => k.IlgiliCalisan != null && k.IlgiliCalisan.AdSoyad.Contains(arama));
        }

        if (request.Tablo is { Length: > 0 } tablolar)
        {
            kayitlar = kayitlar.Where(k => tablolar.Contains(k.Tablo));
        }

        if (request.Islem is { Length: > 0 } islemler)
        {
            kayitlar = kayitlar.Where(k => islemler.Contains(k.Islem));
        }

        if (request.Baslangic is { } baslangic)
        {
            var sinir = sirketSaati.Utc(baslangic.ToDateTime(TimeOnly.MinValue));
            kayitlar = kayitlar.Where(k => k.Tarih >= sinir);
        }

        if (request.Bitis is { } bitis)
        {
            var sinir = sirketSaati.Utc(bitis.AddDays(1).ToDateTime(TimeOnly.MinValue));
            kayitlar = kayitlar.Where(k => k.Tarih < sinir);
        }

        var sorgu = kayitlar
            .Sirala(request.Sort, SiralamaAlanlari, q => q.OrderByDescending(k => k.Id), k => k.Id)
            .Select(k => new
            {
                Kayit = k,
                KendiKaydi = k.KullaniciId != null
                             && (k.IlgiliKullaniciId == k.KullaniciId
                                 || db.Users.Any(u => u.Id == k.KullaniciId && u.CalisanId != null && u.CalisanId == k.IlgiliCalisanId))
            });

        if (request.SadeceKendiKaydi)
        {
            sorgu = sorgu.Where(x => x.KendiKaydi);
        }

        var sayfa = await sorgu
            .Select(x => new IslemKaydiSatiri(
                x.Kayit.Id,
                x.Kayit.Tarih,
                x.Kayit.KullaniciId,
                x.Kayit.Islem,
                x.Kayit.Tablo,
                x.Kayit.KayitId,
                x.Kayit.IlgiliCalisanId,
                x.Kayit.IlgiliCalisan == null ? null : x.Kayit.IlgiliCalisan.Ad + " " + x.Kayit.IlgiliCalisan.Soyad,
                x.Kayit.IlgiliKullaniciId,
                x.KendiKaydi,
                x.Kayit.Degisiklikler))
            .SayfalaAsync(request.Page, request.PageSize, cancellationToken);
        var degisiklikler = sayfa.Items.ToDictionary(s => s.Id, s => JsonSerializer.Deserialize<List<AlanDegisikligi>>(s.Degisiklikler) ?? []);
        var tumu = degisiklikler.Values.SelectMany(d => d).ToList();
        var calisanIdler = Idler(tumu, CalisanAlanlari);
        var departmanIdler = Idler(tumu, DepartmanAlanlari);
        var calisanAdlari = await db.Calisanlar
            .Where(c => calisanIdler.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Ad + " " + c.Soyad, cancellationToken);
        var departmanAdlari = await db.Departmanlar
            .Where(d => departmanIdler.Contains(d.Id))
            .ToDictionaryAsync(d => d.Id, d => d.Ad, cancellationToken);
        var adlar = await kullaniciAdlari.OkuAsync(
            sayfa.Items.Select(s => s.KullaniciId).Concat(Idler(tumu, KullaniciAlanlari).Select(id => (int?)id)),
            cancellationToken);
        var gunlukDakika = await db.GunlukDakikaAsync(cancellationToken);

        string? Goster(string alan, string? deger)
        {
            if (deger is null || !int.TryParse(deger, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var sayi))
            {
                return deger;
            }

            if (SureAlanlari.Contains(alan))
            {
                return SureMetni.Olustur(sayi, gunlukDakika);
            }

            var ad = CalisanAlanlari.Contains(alan) ? calisanAdlari.GetValueOrDefault(sayi)
                : DepartmanAlanlari.Contains(alan) ? departmanAdlari.GetValueOrDefault(sayi)
                : KullaniciAlanlari.Contains(alan) ? adlar.GetValueOrDefault(sayi)
                : null;
            return ad ?? deger;
        }

        return new PagedResponse<IslemKaydiResponse>(
            sayfa.Items.Select(s => new IslemKaydiResponse(
                s.Id,
                s.Tarih,
                s.KullaniciId,
                s.KullaniciId is { } kullaniciId ? adlar.GetValueOrDefault(kullaniciId, kullaniciId.ToString()) : "Sistem",
                s.Islem,
                s.Tablo,
                s.KayitId,
                s.IlgiliCalisanId,
                s.IlgiliCalisanAdi,
                s.IlgiliKullaniciId,
                s.KendiKaydi,
                degisiklikler[s.Id].Select(d => d with { Eski = Goster(d.Alan, d.Eski), Yeni = Goster(d.Alan, d.Yeni) }).ToList())).ToList(),
            sayfa.TotalCount,
            sayfa.Page,
            sayfa.PageSize);
    }

    private static List<int> Idler(IEnumerable<AlanDegisikligi> degisiklikler, IReadOnlySet<string> alanlar) =>
        degisiklikler
            .Where(d => alanlar.Contains(d.Alan))
            .SelectMany(d => new[] { d.Eski, d.Yeni })
            .Select(deger => int.TryParse(deger, CultureInfo.InvariantCulture, out var id) ? id : (int?)null)
            .OfType<int>()
            .Distinct()
            .ToList();

    private sealed record IslemKaydiSatiri(
        int Id,
        DateTime Tarih,
        int? KullaniciId,
        string Islem,
        string Tablo,
        int? KayitId,
        int? IlgiliCalisanId,
        string? IlgiliCalisanAdi,
        int? IlgiliKullaniciId,
        bool KendiKaydi,
        string Degisiklikler);
}
