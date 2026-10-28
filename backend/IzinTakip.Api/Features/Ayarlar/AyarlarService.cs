using IzinTakip.Domain.Enums;
using IzinTakip.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IzinTakip.Api.Features.Ayarlar;

public sealed class AyarlarService(IzinTakipDbContext db, ILogger<AyarlarService> logger)
{
    public async Task<AyarlarResponse> GetirAsync(CancellationToken cancellationToken) =>
        Yanit(await db.Ayarlar.AsNoTracking().SingleAsync(cancellationToken));

    public async Task<AyarlarResponse> GuncelleAsync(AyarlarGuncelleRequest request, CancellationToken cancellationToken)
    {
        var ayarlar = await db.Ayarlar.SingleAsync(cancellationToken);
        ayarlar.Guncelle(
            request.CalismaGunleri.Aggregate(HaftaGunleri.Yok, (toplam, gun) => toplam | gun),
            request.MesaiBaslangic,
            request.MesaiBitis,
            request.OgleArasiBaslangic,
            request.OgleArasiBitis,
            request.SaatlikIzinAdimDakika,
            request.HatirlatmaGunSayisi);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Is takvimi ayarlari guncellendi {GunlukCalismaDakikasi}", ayarlar.GunlukCalismaDakikasi);
        return Yanit(ayarlar);
    }

    private static AyarlarResponse Yanit(Domain.Entities.Ayarlar ayarlar) => new(
        Enum.GetValues<HaftaGunleri>().Where(g => g != HaftaGunleri.Yok && ayarlar.CalismaGunleri.HasFlag(g)).ToList(),
        ayarlar.MesaiBaslangic,
        ayarlar.MesaiBitis,
        ayarlar.OgleArasiBaslangic,
        ayarlar.OgleArasiBitis,
        ayarlar.SaatlikIzinAdimDakika,
        ayarlar.HatirlatmaGunSayisi,
        ayarlar.GunlukCalismaDakikasi);
}
