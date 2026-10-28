using IzinTakip.Api.Common;
using IzinTakip.Api.Common.Bildirimler;
using IzinTakip.Api.Common.Izin;
using IzinTakip.Api.Common.Kapsam;
using IzinTakip.Domain;
using IzinTakip.Domain.Entities;
using IzinTakip.Domain.Exceptions;
using IzinTakip.Infrastructure.Data;
using IzinTakip.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace IzinTakip.Api.Features.IzinTalepleri;

public sealed class IzinOnayService(
    IzinTakipDbContext db,
    IGecerliKullanici gecerliKullanici,
    CalisanKapsami kapsam,
    OnayciBulucu onayciBulucu,
    BildirimYazici bildirimYazici,
    SirketSaati sirketSaati,
    TimeProvider timeProvider,
    ILogger<IzinOnayService> logger)
{
    public async Task OnaylaAsync(int id, string? not, CancellationToken cancellationToken)
    {
        var yetkiler = await gecerliKullanici.YetkileriAsync(cancellationToken);
        var talep = await KapsamdakiTalepAsync(id, cancellationToken);
        talep.Onayla(
            yetkiler.KullaniciId,
            yetkiler.CalisanId,
            yetkiler.SuperAdmin,
            not,
            timeProvider.GetUtcNow().UtcDateTime,
            talep.IzinTuru.BelgeZorunlu,
            await db.IzinBelgeleri.CountAsync(b => b.IzinTalebiId == id, cancellationToken));

        var ozet = await OzetAsync(talep, cancellationToken);
        bildirimYazici.Ekle(
            await onayciBulucu.CalisaninKendisiAsync(talep.CalisanId, cancellationToken),
            "İzin talebiniz onaylandı",
            NotEkle(ozet, talep.KararNotu, "Not"),
            Baglanti(id),
            "Talebi görüntüle");
        await KaydetAsync(cancellationToken);

        logger.LogInformation("Izin talebi onaylandi {IzinTalebiId} {KullaniciId}", id, yetkiler.KullaniciId);
    }

    public async Task ReddetAsync(int id, string gerekce, CancellationToken cancellationToken)
    {
        var yetkiler = await gecerliKullanici.YetkileriAsync(cancellationToken);
        var talep = await KapsamdakiTalepAsync(id, cancellationToken);
        talep.Reddet(yetkiler.KullaniciId, yetkiler.CalisanId, yetkiler.SuperAdmin, gerekce, timeProvider.GetUtcNow().UtcDateTime);

        var ozet = await OzetAsync(talep, cancellationToken);
        bildirimYazici.Ekle(
            await onayciBulucu.CalisaninKendisiAsync(talep.CalisanId, cancellationToken),
            "İzin talebiniz reddedildi",
            NotEkle(ozet, talep.KararNotu, "Gerekçe"),
            Baglanti(id),
            "Talebi görüntüle");
        await KaydetAsync(cancellationToken);

        logger.LogInformation("Izin talebi reddedildi {IzinTalebiId} {KullaniciId}", id, yetkiler.KullaniciId);
    }

    public async Task IptalEtAsync(int id, string? not, CancellationToken cancellationToken)
    {
        var yetkiler = await gecerliKullanici.YetkileriAsync(cancellationToken);
        var talep = await KapsamdakiTalepAsync(id, cancellationToken);
        var kendiTalebi = talep.CalisanId == yetkiler.CalisanId;
        if (!kendiTalebi && !yetkiler.VarMi(Yetkiler.IzinTalebi.Yonet))
        {
            throw new YetkiYokException("Başkasının izin talebini iptal etme yetkiniz yok.");
        }

        talep.IptalEt(
            yetkiler.KullaniciId,
            not,
            sirketSaati.Simdi(),
            timeProvider.GetUtcNow().UtcDateTime,
            yetkiler.VarMi(Yetkiler.IzinTalebi.Duzelt));

        var ozet = await OzetAsync(talep, cancellationToken);
        if (kendiTalebi)
        {
            bildirimYazici.Ekle(
                (await onayciBulucu.OnaycilarAsync(talep.CalisanId, cancellationToken)).Where(a => a.KullaniciId != yetkiler.KullaniciId),
                "İzin talebi iptal edildi",
                $"{talep.Calisan.Ad} {talep.Calisan.Soyad} izin talebini iptal etti. {ozet}",
                Baglanti(id),
                "Talebi görüntüle");
        }
        else
        {
            bildirimYazici.Ekle(
                await onayciBulucu.CalisaninKendisiAsync(talep.CalisanId, cancellationToken),
                "İzin talebiniz iptal edildi",
                NotEkle(ozet, talep.KararNotu, "Not"),
                Baglanti(id),
                "Talebi görüntüle");
        }

        await KaydetAsync(cancellationToken);
        logger.LogInformation("Izin talebi iptal edildi {IzinTalebiId} {KullaniciId}", id, yetkiler.KullaniciId);
    }

    private async Task<IzinTalebi> KapsamdakiTalepAsync(int id, CancellationToken cancellationToken)
    {
        var calisanlar = await kapsam.CalisanlarAsync(cancellationToken);
        return await db.IzinTalepleri
            .Include(t => t.IzinTuru)
            .Include(t => t.Calisan)
            .SingleOrDefaultAsync(t => t.Id == id && calisanlar.Any(c => c.Id == t.CalisanId), cancellationToken)
            ?? throw new KayitBulunamadiException("İzin talebi bulunamadı.");
    }

    private async Task<string> OzetAsync(IzinTalebi talep, CancellationToken cancellationToken)
    {
        var gunluk = (await db.Ayarlar.AsNoTracking().SingleAsync(cancellationToken)).GunlukCalismaDakikasi;
        return TalepOzeti.Olustur(talep, talep.IzinTuru.Ad, gunluk);
    }

    private async Task KaydetAsync(CancellationToken cancellationToken)
    {
        await db.SaveChangesAsync(cancellationToken);
        bildirimYazici.EpostalariKuyrugaAl();
    }

    private static string NotEkle(string ozet, string? not, string etiket) =>
        string.IsNullOrWhiteSpace(not) ? ozet : $"{ozet}. {etiket}: {not}";

    private static string Baglanti(int id) => $"/izin-talepleri/{id}";
}
