using IzinTakip.Api.Common;
using IzinTakip.Api.Common.Izin;
using IzinTakip.Api.Common.Notifications;
using IzinTakip.Domain.Enums;
using IzinTakip.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IzinTakip.Api.BackgroundServices;

public sealed class OnayHatirlatmaJob(
    IServiceScopeFactory scopeFactory,
    BackgroundJobOptions ayarlar,
    SirketSaati sirketSaati,
    TimeProvider timeProvider,
    ILogger<OnayHatirlatmaJob> logger) : DailyJob(sirketSaati, timeProvider, logger)
{
    protected override TimeOnly Saat => ayarlar.HatirlatmaSaati;

    public override async Task CalistirAsync(DateOnly bugun, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IzinTakipDbContext>();
        var onayciBulucu = scope.ServiceProvider.GetRequiredService<OnayciBulucu>();
        var bildirimYazici = scope.ServiceProvider.GetRequiredService<NotificationPublisher>();

        var ayar = await db.Ayarlar.AsNoTracking().SingleAsync(cancellationToken);
        var sonGun = bugun.AddDays(-ayar.HatirlatmaGunSayisi);
        var acilisSiniri = SirketSaati.Utc(sonGun.AddDays(1).ToDateTime(TimeOnly.MinValue));
        var talepler = await db.IzinTalepleri
            .Include(t => t.Calisan)
            .Include(t => t.IzinTuru)
            .Where(t => t.Durum == IzinDurumu.Beklemede
                        && t.Calisan.Aktif
                        && t.OlusturmaTarihi < acilisSiniri
                        && (t.SonHatirlatmaTarihi == null || t.SonHatirlatmaTarihi <= sonGun))
            .ToListAsync(cancellationToken);
        var onaycilar = await onayciBulucu.OnaycilarAsync(talepler.Select(t => t.CalisanId).Distinct().ToList(), cancellationToken);

        foreach (var talep in talepler)
        {
            var bekledigiGun = bugun.DayNumber - DateOnly.FromDateTime(SirketSaati.Yerel(talep.OlusturmaTarihi)).DayNumber;
            bildirimYazici.Ekle(
                onaycilar[talep.CalisanId],
                "Hatırlatma: onayınızı bekleyen izin talebi",
                $"{talep.Calisan.Ad} {talep.Calisan.Soyad} adlı çalışanın izin talebi {bekledigiGun} gündür onay bekliyor. "
                + TalepOzeti.Olustur(talep, talep.IzinTuru.Ad, ayar.GunlukCalismaDakikasi),
                TalepOzeti.Baglanti(talep.Id),
                "Talebi incele");
            talep.HatirlatmaYapildi(bugun);
        }

        await db.SaveChangesAsync(cancellationToken);
        bildirimYazici.EpostalariKuyrugaAl();
        logger.LogInformation("Bekleyen talepler icin hatirlatma gonderildi {TalepSayisi} {Tarih}", talepler.Count, bugun);
    }
}
