using System.Data;
using IzinTakip.Api.Common;
using IzinTakip.Api.Common.Izin;
using IzinTakip.Api.Common.Kapsam;
using IzinTakip.Api.Common.Paging;
using IzinTakip.Domain;
using IzinTakip.Domain.Entities;
using IzinTakip.Domain.Enums;
using IzinTakip.Domain.Exceptions;
using IzinTakip.Domain.Izin;
using IzinTakip.Infrastructure.Data;
using IzinTakip.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace IzinTakip.Api.Features.IzinTalepleri;

public sealed class IzinTalebiService(
    IzinTakipDbContext db,
    IGecerliKullanici gecerliKullanici,
    CalisanKapsami kapsam,
    BakiyeSorgusu bakiyeSorgusu,
    CalismaTakvimiOkuyucu takvimOkuyucu,
    SirketSaati sirketSaati,
    ILogger<IzinTalebiService> logger)
{
    public async Task<IzinTalebiResponse> OlusturAsync(IzinTalebiOlusturRequest request, CancellationToken cancellationToken)
    {
        var yetkiler = await gecerliKullanici.YetkileriAsync(cancellationToken);
        var calisanId = await TalepSahibiAsync(request.CalisanId, yetkiler, cancellationToken);
        var tur = await TurAsync(request.IzinTuruId, cancellationToken);
        var bitisTarihi = request.BitisTarihi ?? request.Tarih;

        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var takvim = await takvimOkuyucu.OkuAsync(request.Tarih, bitisTarihi, cancellationToken);
        var pencereBaslangic = request.Tarih.ToDateTime(TimeOnly.MinValue);
        var pencereBitis = bitisTarihi.AddDays(1).ToDateTime(TimeOnly.MinValue);
        var acikTalepler = await AcikTalepler(calisanId)
            .Where(t => t.Baslangic < pencereBitis && t.Bitis > pencereBaslangic)
            .Select(t => new ZamanAraligi(t.Baslangic, t.Bitis))
            .ToListAsync(cancellationToken);
        var bakiye = await bakiyeSorgusu.HesaplaAsync(calisanId, cancellationToken);
        var turunYilIcindeKullanimi = await AcikTalepler(calisanId)
            .Where(t => t.IzinTuruId == tur.Id && t.Baslangic.Year == request.Tarih.Year)
            .SumAsync(t => t.Dakika, cancellationToken);

        var talep = IzinTalebi.Olustur(
            calisanId,
            tur,
            Sure(request),
            string.IsNullOrWhiteSpace(request.Aciklama) ? null : request.Aciklama.Trim(),
            takvim,
            acikTalepler,
            bakiye,
            turunYilIcindeKullanimi,
            sirketSaati.Simdi(),
            yetkiler.VarMi(Yetkiler.IzinTalebi.Duzelt));
        db.IzinTalepleri.Add(talep);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        logger.LogInformation("Izin talebi olusturuldu {IzinTalebiId} {CalisanId} {Dakika}", talep.Id, calisanId, talep.Dakika);
        return await DetayAsync(talep.Id, cancellationToken);
    }

    public async Task<SureHesaplaResponse> SureHesaplaAsync(SureHesaplaRequest request, CancellationToken cancellationToken)
    {
        var tur = await TurAsync(request.IzinTuruId, cancellationToken);
        var takvim = await takvimOkuyucu.OkuAsync(request.Tarih, request.BitisTarihi ?? request.Tarih, cancellationToken);
        var hesap = IzinSuresiHesaplayici.Hesapla(Sure(request), tur.GunHesabi, takvim);
        return new SureHesaplaResponse(hesap.Dakika, SureMetni.Olustur(hesap.Dakika, takvim.GunlukDakika), hesap.Aralik.Baslangic, hesap.Aralik.Bitis);
    }

    public async Task<PagedResponse<IzinTalebiListeResponse>> ListeleAsync(IzinTalebiListeRequest request, CancellationToken cancellationToken)
    {
        var calisanlar = await kapsam.CalisanlarAsync(cancellationToken);
        var sorgu = db.IzinTalepleri.AsNoTracking().Where(t => calisanlar.Any(c => c.Id == t.CalisanId));
        if (request.CalisanId is not null)
        {
            sorgu = sorgu.Where(t => t.CalisanId == request.CalisanId);
        }

        if (request.Durum is not null)
        {
            sorgu = sorgu.Where(t => t.Durum == request.Durum);
        }

        if (request.IzinTuruId is not null)
        {
            sorgu = sorgu.Where(t => t.IzinTuruId == request.IzinTuruId);
        }

        if (request.Baslangic is { } baslangic)
        {
            var sinir = baslangic.ToDateTime(TimeOnly.MinValue);
            sorgu = sorgu.Where(t => t.Bitis > sinir);
        }

        if (request.Bitis is { } bitis)
        {
            var sinir = bitis.AddDays(1).ToDateTime(TimeOnly.MinValue);
            sorgu = sorgu.Where(t => t.Baslangic < sinir);
        }

        var sayfa = await sorgu
            .OrderByDescending(t => t.Baslangic).ThenByDescending(t => t.Id)
            .Select(t => new IzinTalebiListeResponse(
                t.Id, t.CalisanId, t.Calisan.Ad + " " + t.Calisan.Soyad, t.IzinTuru.Ad, t.SureTipi,
                t.Baslangic, t.Bitis, t.Dakika, string.Empty, t.Durum))
            .SayfalaAsync(request.Page, request.PageSize, cancellationToken);

        var gunluk = await GunlukDakikaAsync(cancellationToken);
        return sayfa with { Items = sayfa.Items.Select(t => t with { SureMetni = SureMetni.Olustur(t.Dakika, gunluk) }).ToList() };
    }

    public async Task<IzinTalebiResponse> DetayAsync(int id, CancellationToken cancellationToken)
    {
        var calisanlar = await kapsam.CalisanlarAsync(cancellationToken);
        var talep = await db.IzinTalepleri
            .AsNoTracking()
            .Where(t => t.Id == id && calisanlar.Any(c => c.Id == t.CalisanId))
            .Select(t => new IzinTalebiResponse(
                t.Id, t.CalisanId, t.Calisan.Ad + " " + t.Calisan.Soyad, t.Calisan.Departman.Ad, t.IzinTuruId, t.IzinTuru.Ad,
                t.SureTipi, t.Baslangic, t.Bitis, t.Dakika, string.Empty, t.Durum, t.Aciklama, t.OlusturmaTarihi,
                t.KararVerenKullaniciId, t.KararTarihi, t.KararNotu))
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new KayitBulunamadiException("İzin talebi bulunamadı.");

        return talep with { SureMetni = SureMetni.Olustur(talep.Dakika, await GunlukDakikaAsync(cancellationToken)) };
    }

    private async Task<int> TalepSahibiAsync(int? istenenCalisanId, KullaniciYetkileri yetkiler, CancellationToken cancellationToken)
    {
        if (istenenCalisanId is null || istenenCalisanId == yetkiler.CalisanId)
        {
            if (yetkiler.CalisanId is not { } kendisi)
            {
                throw new IsKuraliException("Hesabınız bir çalışan kaydına bağlı değil; talep için çalışan seçin.");
            }

            if (!yetkiler.VarMi(Yetkiler.IzinTalebi.Olustur))
            {
                throw new YetkiYokException("Kendi adınıza izin talebi açma yetkiniz yok.");
            }

            return kendisi;
        }

        if (!yetkiler.VarMi(Yetkiler.IzinTalebi.Yonet))
        {
            throw new YetkiYokException("Başkası adına izin talebi açma yetkiniz yok.");
        }

        var calisan = await kapsam.BulAsync(istenenCalisanId.Value, cancellationToken);
        if (!calisan.Aktif)
        {
            throw new IsKuraliException("Pasif çalışan adına izin talebi açılamaz.");
        }

        return calisan.Id;
    }

    private async Task<IzinTuru> TurAsync(int izinTuruId, CancellationToken cancellationToken) =>
        await db.IzinTurleri.AsNoTracking().SingleOrDefaultAsync(t => t.Id == izinTuruId, cancellationToken)
        ?? throw new KayitBulunamadiException("İzin türü bulunamadı.");

    private IQueryable<IzinTalebi> AcikTalepler(int calisanId) =>
        db.IzinTalepleri.Where(t => t.CalisanId == calisanId && (t.Durum == IzinDurumu.Beklemede || t.Durum == IzinDurumu.Onaylandi));

    private async Task<int> GunlukDakikaAsync(CancellationToken cancellationToken) =>
        (await db.Ayarlar.AsNoTracking().SingleAsync(cancellationToken)).GunlukCalismaDakikasi;

    private static SureIstegi Sure(SureHesaplaRequest request) => new(
        request.SureTipi, request.Tarih, request.BitisTarihi, request.GunYarisi, request.BaslangicSaati, request.BitisSaati);
}
