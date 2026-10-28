using IzinTakip.Api.Common;
using IzinTakip.Domain.Enums;
using IzinTakip.Domain.Izin;
using IzinTakip.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IzinTakip.Api.BackgroundServices;

public sealed class YillikHakServisi(
    IServiceScopeFactory scopeFactory,
    ArkaPlanAyarlari ayarlar,
    SirketSaati sirketSaati,
    TimeProvider timeProvider,
    ILogger<YillikHakServisi> logger) : GunlukGorev(sirketSaati, timeProvider, logger)
{
    protected override TimeOnly Saat => ayarlar.YillikHakSaati;

    public override async Task CalistirAsync(DateOnly bugun, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IzinTakipDbContext>();

        var kurallar = await db.IzinHakkiKurallari.AsNoTracking().Where(k => k.Aktif).ToListAsync(cancellationToken);
        var gunluk = (await db.Ayarlar.AsNoTracking().SingleAsync(cancellationToken)).GunlukCalismaDakikasi;
        var birYilOnce = bugun.AddYears(-1);
        var calisanlar = await db.Calisanlar
            .AsNoTracking()
            .Where(c => c.Aktif && c.IseGirisTarihi <= birYilOnce)
            .ToListAsync(cancellationToken);
        var yazilmislar = (await db.IzinHaklari
                .Where(h => h.Kaynak == HakKaynagi.Kural && db.Calisanlar.Any(c => c.Id == h.CalisanId && c.Aktif))
                .Select(h => new { h.CalisanId, h.KazanimTarihi })
                .ToListAsync(cancellationToken))
            .ToLookup(h => h.CalisanId, h => h.KazanimTarihi);

        var toplam = 0;
        foreach (var calisan in calisanlar)
        {
            var haklar = IzinHakkiHesaplayici.EksikHaklar(
                calisan,
                DateOnly.FromDateTime(SirketSaati.Yerel(calisan.OlusturmaTarihi)),
                bugun,
                yazilmislar[calisan.Id].ToHashSet(),
                kurallar,
                gunluk);
            db.IzinHaklari.AddRange(haklar);
            toplam += haklar.Count;
        }

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Yillik hak satirlari yazildi {Sayi} {Tarih}", toplam, bugun);
    }
}
