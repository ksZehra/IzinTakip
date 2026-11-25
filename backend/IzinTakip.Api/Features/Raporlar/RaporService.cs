using IzinTakip.Api.Common;
using IzinTakip.Api.Common.Auth;
using IzinTakip.Api.Common.Izin;
using IzinTakip.Api.Common.Paging;
using IzinTakip.Domain;
using IzinTakip.Domain.Entities;
using IzinTakip.Domain.Enums;
using IzinTakip.Domain.Izin;
using IzinTakip.Infrastructure.Data;
using IzinTakip.Infrastructure.Excel;
using Microsoft.EntityFrameworkCore;

namespace IzinTakip.Api.Features.Raporlar;

public sealed class RaporService(
    IzinTakipDbContext db,
    CalisanScope kapsam,
    BakiyeQuery bakiyeSorgusu,
    SirketSaati sirketSaati)
{
    public async Task<IReadOnlyList<TakvimKaydiResponse>> TakvimAsync(TakvimRequest request, CancellationToken cancellationToken)
    {
        var calisanlar = await KapsamdakilerAsync(request.DepartmanId, cancellationToken);
        var baslangic = request.Baslangic.ToDateTime(TimeOnly.MinValue);
        var bitis = request.Bitis.AddDays(1).ToDateTime(TimeOnly.MinValue);

        return await db.IzinTalepleri
            .AsNoTracking()
            .Where(t => calisanlar.Any(c => c.Id == t.CalisanId)
                        && (t.Durum == IzinDurumu.Beklemede || t.Durum == IzinDurumu.Onaylandi)
                        && t.Baslangic < bitis
                        && t.Bitis > baslangic)
            .OrderBy(t => t.Baslangic).ThenBy(t => t.Id)
            .Select(t => new TakvimKaydiResponse(
                t.Id, t.CalisanId, t.Calisan.Ad + " " + t.Calisan.Soyad, t.Calisan.Departman.Ad, t.IzinTuru.Ad,
                t.SureTipi, t.Baslangic, t.Bitis, t.Durum))
            .ToListAsync(cancellationToken);
    }

    public static readonly IReadOnlyDictionary<string, Func<IQueryable<Calisan>, bool, IOrderedQueryable<Calisan>>> BakiyeSiralamaAlanlari =
        new Dictionary<string, Func<IQueryable<Calisan>, bool, IOrderedQueryable<Calisan>>>
        {
            ["calisanAdSoyad"] = (sorgu, azalan) => azalan
                ? sorgu.OrderByDescending(c => c.AdSoyad)
                : sorgu.OrderBy(c => c.AdSoyad),
            ["departmanAdi"] = SiralamaExtensions.SiralamaAlani<Calisan, string>(c => c.Departman.Ad),
            ["iseGirisTarihi"] = SiralamaExtensions.SiralamaAlani<Calisan, DateOnly>(c => c.IseGirisTarihi)
        };

    public async Task<PagedResponse<BakiyeRaporuResponse>> BakiyelerAsync(BakiyeRaporuRequest request, CancellationToken cancellationToken)
    {
        var sayfa = await Satirlar(await BakiyeFiltreliAsync(request, cancellationToken), request.Sort)
            .SayfalaAsync(request.Page, request.PageSize, cancellationToken);
        var idler = sayfa.Items.Select(s => s.CalisanId).ToList();
        var bakiyeler = await bakiyeSorgusu.HesaplaAsync(db.Calisanlar.Where(c => idler.Contains(c.Id)), cancellationToken);
        var gunluk = await db.GunlukDakikaAsync(cancellationToken);

        return new PagedResponse<BakiyeRaporuResponse>(
            sayfa.Items.Select(s => Yanit(s, bakiyeler[s.CalisanId], gunluk)).ToList(),
            sayfa.TotalCount,
            sayfa.Page,
            sayfa.PageSize);
    }

    public async Task<DosyaResponse> BakiyeExcelAsync(BakiyeRaporuFiltreRequest request, CancellationToken cancellationToken)
    {
        var calisanlar = await BakiyeFiltreliAsync(request, cancellationToken);
        var satirlar = await Satirlar(calisanlar, request.Sort).ToListAsync(cancellationToken);
        var bakiyeler = await bakiyeSorgusu.HesaplaAsync(calisanlar, cancellationToken);
        var gunluk = await db.GunlukDakikaAsync(cancellationToken);

        var icerik = ExcelWriter.Yaz(
            "Bakiyeler",
            [
                new ExcelColumn<BakiyeRaporuResponse>("Çalışan", r => r.CalisanAdSoyad, 28),
                new ExcelColumn<BakiyeRaporuResponse>("Departman", r => r.DepartmanAdi, 22),
                new ExcelColumn<BakiyeRaporuResponse>("İşe Giriş", r => r.IseGirisTarihi.ToDateTime(TimeOnly.MinValue), 12, "dd.MM.yyyy"),
                new ExcelColumn<BakiyeRaporuResponse>("Hak", r => r.HakMetni, 18),
                new ExcelColumn<BakiyeRaporuResponse>("Kullanılan", r => r.KullanilanMetni, 18),
                new ExcelColumn<BakiyeRaporuResponse>("Bekleyen", r => r.BekleyenMetni, 18),
                new ExcelColumn<BakiyeRaporuResponse>("Kullanılabilir", r => r.KullanilabilirMetni, 18)
            ],
            satirlar.Select(s => Yanit(s, bakiyeler[s.CalisanId], gunluk)));

        return new DosyaResponse($"bakiye-raporu-{sirketSaati.Bugun():yyyyMMdd}.xlsx", ExcelWriter.IcerikTuru, icerik);
    }

    private async Task<IQueryable<Calisan>> KapsamdakilerAsync(int? departmanId, CancellationToken cancellationToken)
    {
        var calisanlar = await kapsam.CalisanlarAsync(cancellationToken);
        return departmanId is null ? calisanlar : calisanlar.Where(c => c.DepartmanId == departmanId);
    }

    private async Task<IQueryable<Calisan>> BakiyeFiltreliAsync(BakiyeRaporuFiltreRequest request, CancellationToken cancellationToken)
    {
        var sorgu = (await kapsam.CalisanlarAsync(cancellationToken)).Where(c => c.Aktif);
        if (!string.IsNullOrWhiteSpace(request.Arama))
        {
            var arama = request.Arama.Trim();
            sorgu = sorgu.Where(c => c.AdSoyad.Contains(arama));
        }

        if (request.DepartmanId is { Length: > 0 } departmanlar)
        {
            sorgu = sorgu.Where(c => departmanlar.Contains(c.DepartmanId));
        }

        if (request.IseGirisBaslangic is not null)
        {
            sorgu = sorgu.Where(c => c.IseGirisTarihi >= request.IseGirisBaslangic);
        }

        if (request.IseGirisBitis is not null)
        {
            sorgu = sorgu.Where(c => c.IseGirisTarihi <= request.IseGirisBitis);
        }

        return sorgu;
    }

    // Bakiye tutarları veritabanında değil bellekte hesaplandığı için yalnız çalışan alanlarıyla sıralanır.
    private static IQueryable<CalisanSatiri> Satirlar(IQueryable<Calisan> calisanlar, string? sort) =>
        calisanlar
            .AsNoTracking()
            .Sirala(sort, BakiyeSiralamaAlanlari, q => q.OrderBy(c => c.AdSoyad).ThenBy(c => c.Id))
            .Select(c => new CalisanSatiri(c.Id, c.Ad + " " + c.Soyad, c.Departman.Ad, c.IseGirisTarihi));

    private static BakiyeRaporuResponse Yanit(CalisanSatiri satir, Bakiye bakiye, int gunluk) => new(
        satir.CalisanId, satir.AdSoyad, satir.DepartmanAdi, satir.IseGirisTarihi,
        bakiye.HakDakika, SureMetni.Olustur(bakiye.HakDakika, gunluk),
        bakiye.KullanilanDakika, SureMetni.Olustur(bakiye.KullanilanDakika, gunluk),
        bakiye.BekleyenDakika, SureMetni.Olustur(bakiye.BekleyenDakika, gunluk),
        bakiye.KullanilabilirDakika, SureMetni.Olustur(bakiye.KullanilabilirDakika, gunluk));

    private sealed record CalisanSatiri(int CalisanId, string AdSoyad, string DepartmanAdi, DateOnly IseGirisTarihi);
}
