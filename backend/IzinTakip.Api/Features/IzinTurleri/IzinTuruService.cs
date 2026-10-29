using IzinTakip.Api.Common.Izin;
using IzinTakip.Domain;
using IzinTakip.Domain.Entities;
using IzinTakip.Domain.Exceptions;
using IzinTakip.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IzinTakip.Api.Features.IzinTurleri;

public sealed class IzinTuruService(IzinTakipDbContext db, ILogger<IzinTuruService> logger)
{
    public async Task<IReadOnlyList<IzinTuruResponse>> ListeleAsync(bool? aktif, CancellationToken cancellationToken)
    {
        var sorgu = db.IzinTurleri.AsNoTracking();
        if (aktif is not null)
        {
            sorgu = sorgu.Where(t => t.Aktif == aktif);
        }

        var turler = await sorgu.OrderBy(t => t.Ad).ToListAsync(cancellationToken);
        var gunluk = await db.GunlukDakikaAsync(cancellationToken);
        return turler.Select(t => Yanit(t, gunluk)).ToList();
    }

    public async Task<IzinTuruResponse> DetayAsync(int id, CancellationToken cancellationToken)
    {
        var tur = await db.IzinTurleri.AsNoTracking().SingleOrDefaultAsync(t => t.Id == id, cancellationToken)
            ?? throw new NotFoundException("İzin türü bulunamadı.");
        return Yanit(tur, await db.GunlukDakikaAsync(cancellationToken));
    }

    public async Task<IzinTuruResponse> OlusturAsync(IzinTuruKaydetRequest request, CancellationToken cancellationToken)
    {
        var ad = request.Ad.Trim();
        await AdBenzersizMiAsync(null, ad, cancellationToken);

        var tur = new IzinTuru(
            ad, request.BakiyedenDuser, request.BelgeZorunlu, request.GunHesabi, request.TamGunAlinir,
            request.YarimGunAlinir, request.SaatlikAlinir, request.TalepBasinaEnFazlaDakika, request.YillikEnFazlaDakika);
        db.IzinTurleri.Add(tur);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Izin turu olusturuldu {IzinTuruId}", tur.Id);
        return await DetayAsync(tur.Id, cancellationToken);
    }

    public async Task<IzinTuruResponse> GuncelleAsync(int id, IzinTuruKaydetRequest request, CancellationToken cancellationToken)
    {
        var tur = await BulAsync(id, cancellationToken);
        var ad = request.Ad.Trim();
        await AdBenzersizMiAsync(id, ad, cancellationToken);

        tur.Guncelle(
            ad, request.BakiyedenDuser, request.BelgeZorunlu, request.GunHesabi, request.TamGunAlinir,
            request.YarimGunAlinir, request.SaatlikAlinir, request.TalepBasinaEnFazlaDakika, request.YillikEnFazlaDakika);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Izin turu guncellendi {IzinTuruId}", id);
        return await DetayAsync(id, cancellationToken);
    }

    public async Task<IzinTuruResponse> AktiflikDegistirAsync(int id, bool aktif, CancellationToken cancellationToken)
    {
        var tur = await BulAsync(id, cancellationToken);
        tur.AktiflikDegistir(aktif);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Izin turu aktifligi degisti {IzinTuruId} {Aktif}", id, aktif);
        return await DetayAsync(id, cancellationToken);
    }

    private async Task<IzinTuru> BulAsync(int id, CancellationToken cancellationToken) =>
        await db.IzinTurleri.SingleOrDefaultAsync(t => t.Id == id, cancellationToken)
        ?? throw new NotFoundException("İzin türü bulunamadı.");

    private async Task AdBenzersizMiAsync(int? id, string ad, CancellationToken cancellationToken)
    {
        if (await db.IzinTurleri.AnyAsync(t => t.Ad == ad && t.Id != id, cancellationToken))
        {
            throw new BusinessRuleException("Bu adla bir izin türü zaten var.");
        }
    }

    private static IzinTuruResponse Yanit(IzinTuru t, int gunluk) => new(
        t.Id,
        t.Ad,
        t.BakiyedenDuser,
        t.BelgeZorunlu,
        t.GunHesabi,
        t.TamGunAlinir,
        t.YarimGunAlinir,
        t.SaatlikAlinir,
        t.TalepBasinaEnFazlaDakika,
        t.TalepBasinaEnFazlaDakika is { } talep ? SureMetni.Olustur(talep, gunluk) : null,
        t.YillikEnFazlaDakika,
        t.YillikEnFazlaDakika is { } yillik ? SureMetni.Olustur(yillik, gunluk) : null,
        t.Aktif);
}
