using IzinTakip.Api.Common;
using IzinTakip.Api.Common.Auth;
using IzinTakip.Api.Common.Izin;
using IzinTakip.Api.Common.Paging;
using IzinTakip.Domain;
using IzinTakip.Domain.Entities;
using IzinTakip.Domain.Exceptions;
using IzinTakip.Domain.Izin;
using IzinTakip.Infrastructure.Data;
using IzinTakip.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace IzinTakip.Api.Features.Calisanlar;

public sealed class CalisanService(
    IzinTakipDbContext db,
    CalisanScope kapsam,
    KullaniciYetkiProvider yetkiOkuyucu,
    ICurrentUser gecerliKullanici,
    KullaniciKorumasi koruma,
    BakiyeQuery bakiyeSorgusu,
    SirketSaati sirketSaati,
    ILogger<CalisanService> logger)
{
    public static readonly IReadOnlyDictionary<string, Func<IQueryable<Calisan>, bool, IOrderedQueryable<Calisan>>> SiralamaAlanlari =
        new Dictionary<string, Func<IQueryable<Calisan>, bool, IOrderedQueryable<Calisan>>>
        {
            ["adSoyad"] = (sorgu, azalan) => azalan
                ? sorgu.OrderByDescending(c => c.AdSoyad)
                : sorgu.OrderBy(c => c.AdSoyad),
            ["departmanAdi"] = SiralamaExtensions.SiralamaAlani<Calisan, string>(c => c.Departman.Ad),
            ["iseGirisTarihi"] = SiralamaExtensions.SiralamaAlani<Calisan, DateOnly>(c => c.IseGirisTarihi),
            ["aktif"] = SiralamaExtensions.SiralamaAlani<Calisan, bool>(c => c.Aktif)
        };

    public async Task<PagedResponse<CalisanListeResponse>> ListeleAsync(CalisanListeRequest request, CancellationToken cancellationToken)
    {
        var sorgu = (await kapsam.CalisanlarAsync(cancellationToken)).AsNoTracking();
        if (!string.IsNullOrWhiteSpace(request.Arama))
        {
            var arama = request.Arama.Trim();
            sorgu = sorgu.Where(c => c.AdSoyad.Contains(arama));
        }

        if (request.DepartmanId is { Length: > 0 } departmanlar)
        {
            sorgu = sorgu.Where(c => departmanlar.Contains(c.DepartmanId));
        }

        if (request.Aktif is not null)
        {
            sorgu = sorgu.Where(c => c.Aktif == request.Aktif);
        }

        if (request.IseGirisBaslangic is not null)
        {
            sorgu = sorgu.Where(c => c.IseGirisTarihi >= request.IseGirisBaslangic);
        }

        if (request.IseGirisBitis is not null)
        {
            sorgu = sorgu.Where(c => c.IseGirisTarihi <= request.IseGirisBitis);
        }

        return await sorgu
            .Sirala(request.Sort, SiralamaAlanlari, q => q.OrderBy(c => c.AdSoyad).ThenBy(c => c.Id))
            .Select(c => new CalisanListeResponse(c.Id, c.Ad, c.Soyad, c.DepartmanId, c.Departman.Ad, c.IseGirisTarihi, c.Aktif))
            .SayfalaAsync(request.Page, request.PageSize, cancellationToken);
    }

    public async Task<CalisanResponse> DetayAsync(int id, CancellationToken cancellationToken) =>
        await (await kapsam.CalisanlarAsync(cancellationToken))
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CalisanResponse(
                c.Id,
                c.Ad,
                c.Soyad,
                c.DepartmanId,
                c.Departman.Ad,
                c.IseGirisTarihi,
                c.DogumTarihi,
                c.HakBaslangicKidemYili,
                c.Aktif,
                db.Users.Where(k => k.CalisanId == c.Id).Select(k => k.Email).FirstOrDefault()))
            .SingleOrDefaultAsync(cancellationToken)
        ?? throw new NotFoundException("Çalışan bulunamadı.");

    public async Task<CalisanResponse> OlusturAsync(CalisanKaydetRequest request, CancellationToken cancellationToken)
    {
        await DepartmanAktifMiAsync(request.DepartmanId, cancellationToken);

        var calisan = new Calisan(
            request.Ad.Trim(),
            request.Soyad.Trim(),
            request.DepartmanId,
            request.IseGirisTarihi!.Value,
            request.DogumTarihi!.Value,
            request.HakBaslangicKidemYili ?? IzinHakkiCalculator.IlkHakKidemYili(request.IseGirisTarihi.Value, sirketSaati.Bugun()));
        db.Calisanlar.Add(calisan);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Calisan olusturuldu {CalisanId}", calisan.Id);
        return await DetayAsync(calisan.Id, cancellationToken);
    }

    public async Task<CalisanResponse> GuncelleAsync(int id, CalisanKaydetRequest request, CancellationToken cancellationToken)
    {
        var calisan = await kapsam.BulAsync(id, cancellationToken);
        if (calisan.DepartmanId != request.DepartmanId)
        {
            await DepartmanAktifMiAsync(request.DepartmanId, cancellationToken);
        }

        calisan.Guncelle(
            request.Ad.Trim(),
            request.Soyad.Trim(),
            request.DepartmanId,
            request.IseGirisTarihi!.Value,
            request.DogumTarihi!.Value,
            request.HakBaslangicKidemYili ?? calisan.HakBaslangicKidemYili);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Calisan guncellendi {CalisanId}", id);
        return await DetayAsync(id, cancellationToken);
    }

    public async Task<CalisanResponse> AktiflikDegistirAsync(int id, bool aktif, CancellationToken cancellationToken)
    {
        var calisan = await kapsam.BulAsync(id, cancellationToken);
        Kullanici? kullanici = null;
        if (!aktif)
        {
            if (await db.Departmanlar.AnyAsync(d => d.YoneticiId == id && d.Aktif, cancellationToken))
            {
                throw new BusinessRuleException("Bu çalışan bir departmanın yöneticisi. Önce yöneticiliği başka birine verin.");
            }

            kullanici = await db.Users.SingleOrDefaultAsync(k => k.CalisanId == id, cancellationToken);
            if (kullanici is not null)
            {
                await koruma.DokunabilirMiAsync(kullanici.Id, await gecerliKullanici.YetkileriAsync(cancellationToken), cancellationToken);
                if (await koruma.SonAktifSuperAdminMiAsync(kullanici.Id, cancellationToken))
                {
                    throw new BusinessRuleException("Bu çalışanın hesabı son aktif SuperAdmin; çalışan pasifleştirilemez.");
                }

                kullanici.Aktif = false;
            }
        }

        calisan.AktiflikDegistir(aktif);
        await db.SaveChangesAsync(cancellationToken);

        if (kullanici is not null)
        {
            yetkiOkuyucu.Unut(kullanici.Id);
        }

        logger.LogInformation("Calisan aktifligi degisti {CalisanId} {Aktif}", id, aktif);
        return await DetayAsync(id, cancellationToken);
    }

    public async Task<IzinHaklariResponse> HaklariAsync(int id, CancellationToken cancellationToken)
    {
        await kapsam.BulAsync(id, cancellationToken);
        var gunluk = await db.GunlukDakikaAsync(cancellationToken);
        var satirlar = await db.IzinHaklari
            .AsNoTracking()
            .Where(h => h.CalisanId == id)
            .OrderBy(h => h.KazanimTarihi).ThenBy(h => h.Id)
            .ToListAsync(cancellationToken);

        var toplam = satirlar.Sum(h => h.Dakika);
        return new IzinHaklariResponse(
            satirlar.Select(h => new IzinHakkiResponse(h.Id, h.KazanimTarihi, h.Dakika, SureMetni.Olustur(h.Dakika, gunluk), h.Kaynak, h.KidemYili, h.Aciklama)).ToList(),
            toplam,
            SureMetni.Olustur(toplam, gunluk));
    }

    public async Task<IzinHaklariResponse> HakEkleAsync(int id, IzinHakkiKaydetRequest request, CancellationToken cancellationToken)
    {
        await kapsam.BulAsync(id, cancellationToken);
        db.IzinHaklari.Add(IzinHakki.Elle(id, request.KazanimTarihi ?? sirketSaati.Bugun(), request.Dakika, request.Aciklama.Trim()));
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Elle hak satiri eklendi {CalisanId} {Dakika}", id, request.Dakika);
        return await HaklariAsync(id, cancellationToken);
    }

    public async Task<IzinHaklariResponse> HakGuncelleAsync(int id, int hakId, IzinHakkiKaydetRequest request, CancellationToken cancellationToken)
    {
        var hak = await HakBulAsync(id, hakId, cancellationToken);
        hak.Duzelt(request.KazanimTarihi ?? hak.KazanimTarihi, request.Dakika, request.Aciklama.Trim());
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Elle hak satiri guncellendi {CalisanId} {HakId} {Dakika}", id, hakId, request.Dakika);
        return await HaklariAsync(id, cancellationToken);
    }

    public async Task HakSilAsync(int id, int hakId, CancellationToken cancellationToken)
    {
        var hak = await HakBulAsync(id, hakId, cancellationToken);
        hak.ElleOldugunuDogrula();
        db.IzinHaklari.Remove(hak);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Elle hak satiri silindi {CalisanId} {HakId}", id, hakId);
    }

    public async Task<BakiyeResponse> BakiyeAsync(int id, CancellationToken cancellationToken)
    {
        await kapsam.BulAsync(id, cancellationToken);
        var bakiye = await bakiyeSorgusu.HesaplaAsync(id, cancellationToken);
        var gunluk = await db.GunlukDakikaAsync(cancellationToken);
        return new BakiyeResponse(
            bakiye.HakDakika, SureMetni.Olustur(bakiye.HakDakika, gunluk),
            bakiye.KullanilanDakika, SureMetni.Olustur(bakiye.KullanilanDakika, gunluk),
            bakiye.BekleyenDakika, SureMetni.Olustur(bakiye.BekleyenDakika, gunluk),
            bakiye.KullanilabilirDakika, SureMetni.Olustur(bakiye.KullanilabilirDakika, gunluk));
    }

    private async Task<IzinHakki> HakBulAsync(int calisanId, int hakId, CancellationToken cancellationToken)
    {
        await kapsam.BulAsync(calisanId, cancellationToken);
        return await db.IzinHaklari.SingleOrDefaultAsync(h => h.Id == hakId && h.CalisanId == calisanId, cancellationToken)
            ?? throw new NotFoundException("Hak satırı bulunamadı.");
    }

    private async Task DepartmanAktifMiAsync(int departmanId, CancellationToken cancellationToken)
    {
        if (!await db.Departmanlar.AnyAsync(d => d.Id == departmanId && d.Aktif, cancellationToken))
        {
            throw new BusinessRuleException("Seçilen departman bulunamadı ya da pasif.");
        }
    }
}
