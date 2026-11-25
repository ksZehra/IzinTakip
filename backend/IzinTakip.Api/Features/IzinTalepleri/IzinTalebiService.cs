using System.Data;
using IzinTakip.Api.Common;
using IzinTakip.Api.Common.Auth;
using IzinTakip.Api.Common.Izin;
using IzinTakip.Api.Common.Notifications;
using IzinTakip.Api.Common.Paging;
using IzinTakip.Domain;
using IzinTakip.Domain.Entities;
using IzinTakip.Domain.Enums;
using IzinTakip.Domain.Exceptions;
using IzinTakip.Domain.Izin;
using IzinTakip.Infrastructure.Data;
using IzinTakip.Infrastructure.Excel;
using IzinTakip.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace IzinTakip.Api.Features.IzinTalepleri;

public sealed class IzinTalebiService(
    IzinTakipDbContext db,
    ICurrentUser gecerliKullanici,
    CalisanScope kapsam,
    BakiyeQuery bakiyeSorgusu,
    CalismaTakvimiProvider takvimOkuyucu,
    OnayciBulucu onayciBulucu,
    NotificationPublisher bildirimYazici,
    KullaniciAdlariProvider kullaniciAdlari,
    SirketSaati sirketSaati,
    ILogger<IzinTalebiService> logger)
{
    public static readonly IReadOnlyDictionary<string, Func<IQueryable<IzinTalebi>, bool, IOrderedQueryable<IzinTalebi>>> SiralamaAlanlari =
        new Dictionary<string, Func<IQueryable<IzinTalebi>, bool, IOrderedQueryable<IzinTalebi>>>
        {
            ["calisanAdSoyad"] = (sorgu, azalan) => azalan
                ? sorgu.OrderByDescending(t => t.Calisan.AdSoyad)
                : sorgu.OrderBy(t => t.Calisan.AdSoyad),
            ["izinTuruAdi"] = SiralamaExtensions.SiralamaAlani<IzinTalebi, string>(t => t.IzinTuru.Ad),
            ["baslangic"] = SiralamaExtensions.SiralamaAlani<IzinTalebi, DateTime>(t => t.Baslangic),
            ["dakika"] = SiralamaExtensions.SiralamaAlani<IzinTalebi, int>(t => t.Dakika),
            // Enum değeri yerine ekrandaki etiketin alfabetik sırası: Beklemede, İptal edildi, Onaylandı, Reddedildi.
            ["durum"] = SiralamaExtensions.SiralamaAlani<IzinTalebi, int>(t =>
                t.Durum == IzinDurumu.Beklemede ? 1
                : t.Durum == IzinDurumu.IptalEdildi ? 2
                : t.Durum == IzinDurumu.Onaylandi ? 3
                : 4)
        };

    public async Task<IzinTalebiResponse> OlusturAsync(IzinTalebiOlusturRequest request, CancellationToken cancellationToken)
    {
        var yetkiler = await gecerliKullanici.YetkileriAsync(cancellationToken);
        var calisanId = await TalepSahibiAsync(request.CalisanId, yetkiler, cancellationToken);
        var tur = await TurAsync(request.IzinTuruId, cancellationToken);
        var bitisTarihi = request.BitisTarihi ?? request.Tarih;

        var takvim = await takvimOkuyucu.OkuAsync(request.Tarih, bitisTarihi, cancellationToken);
        var calisanAdi = await db.Calisanlar.Where(c => c.Id == calisanId).Select(c => c.Ad + " " + c.Soyad).SingleAsync(cancellationToken);
        var onaycilar = (await onayciBulucu.OnaycilarAsync(calisanId, cancellationToken)).Where(a => a.KullaniciId != yetkiler.KullaniciId).ToList();
        var talepSahibi = calisanId == yetkiler.CalisanId ? [] : await onayciBulucu.CalisaninKendisiAsync(calisanId, cancellationToken);

        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var pencereBaslangic = request.Tarih.ToDateTime(TimeOnly.MinValue);
        var pencereBitis = bitisTarihi.AddDays(1).ToDateTime(TimeOnly.MinValue);
        var acikTalepler = (await AcikTalepler(calisanId)
                .Where(t => t.Baslangic < pencereBitis && t.Bitis > pencereBaslangic)
                .Select(t => new { t.SureTipi, t.Baslangic, t.Bitis })
                .ToListAsync(cancellationToken))
            .Select(t => IzinTalebi.CakismaAraligi(t.SureTipi, new ZamanAraligi(t.Baslangic, t.Bitis)))
            .ToList();
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

        var ozet = TalepOzeti.Olustur(talep, tur.Ad, takvim.GunlukDakika);
        var baglanti = TalepOzeti.Baglanti(talep.Id);
        bildirimYazici.Ekle(onaycilar, "Onayınızı bekleyen izin talebi", $"{calisanAdi} izin talebi açtı. {ozet}", baglanti, "Talebi incele");
        bildirimYazici.Ekle(talepSahibi, "Adınıza izin talebi açıldı", ozet, baglanti, "Talebi görüntüle");

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        bildirimYazici.EpostalariKuyrugaAl();

        logger.LogInformation("Izin talebi olusturuldu {IzinTalebiId} {CalisanId} {Dakika}", talep.Id, calisanId, talep.Dakika);
        return await DetayAsync(talep.Id, cancellationToken);
    }

    public async Task<SureHesaplaResponse> SureHesaplaAsync(SureHesaplaRequest request, CancellationToken cancellationToken)
    {
        var tur = await TurAsync(request.IzinTuruId, cancellationToken);
        var takvim = await takvimOkuyucu.OkuAsync(request.Tarih, request.BitisTarihi ?? request.Tarih, cancellationToken);
        var hesap = IzinSuresiCalculator.Hesapla(Sure(request), tur.GunHesabi, takvim);
        return new SureHesaplaResponse(hesap.Dakika, SureMetni.Olustur(hesap.Dakika, takvim.GunlukDakika), hesap.Aralik.Baslangic, hesap.Aralik.Bitis);
    }

    public async Task<PagedResponse<IzinTalebiListeResponse>> ListeleAsync(IzinTalebiListeRequest request, CancellationToken cancellationToken)
    {
        var sayfa = await Sirala(await FiltreliAsync(request, cancellationToken), request.Sort)
            .Select(t => new IzinTalebiListeResponse(
                t.Id, t.CalisanId, t.Calisan.Ad + " " + t.Calisan.Soyad, t.IzinTuru.Ad, t.SureTipi,
                t.Baslangic, t.Bitis, t.Dakika, string.Empty, t.Durum))
            .SayfalaAsync(request.Page, request.PageSize, cancellationToken);

        var gunluk = await db.GunlukDakikaAsync(cancellationToken);
        return sayfa with { Items = sayfa.Items.Select(t => t with { SureMetni = SureMetni.Olustur(t.Dakika, gunluk) }).ToList() };
    }

    public async Task<IzinTalebiResponse> DetayAsync(int id, CancellationToken cancellationToken)
    {
        var calisanlar = await kapsam.CalisanlarAsync(cancellationToken);
        var talep = await Yansit(db.IzinTalepleri.AsNoTracking().Where(t => t.Id == id && calisanlar.Any(c => c.Id == t.CalisanId)))
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("İzin talebi bulunamadı.");

        var adlar = await kullaniciAdlari.OkuAsync([talep.OlusturanKullaniciId, talep.KararVerenKullaniciId], cancellationToken);
        return Tamamla(talep, await db.GunlukDakikaAsync(cancellationToken), adlar);
    }

    public async Task<DosyaResponse> ExcelAsync(IzinTalebiFiltreRequest request, CancellationToken cancellationToken)
    {
        var talepler = await Yansit(Sirala(await FiltreliAsync(request, cancellationToken), request.Sort)).ToListAsync(cancellationToken);
        var gunluk = await db.GunlukDakikaAsync(cancellationToken);
        var adlar = await kullaniciAdlari.OkuAsync(talepler.SelectMany(t => new[] { t.OlusturanKullaniciId, t.KararVerenKullaniciId }), cancellationToken);
        const string tarihSaat = "dd.MM.yyyy HH:mm";

        var icerik = ExcelWriter.Yaz(
            "İzin Talepleri",
            [
                new ExcelColumn<IzinTalebiResponse>("Çalışan", t => t.CalisanAdSoyad, 28),
                new ExcelColumn<IzinTalebiResponse>("Departman", t => t.DepartmanAdi, 22),
                new ExcelColumn<IzinTalebiResponse>("İzin Türü", t => t.IzinTuruAdi, 18),
                new ExcelColumn<IzinTalebiResponse>("Biçim", t => BicimAdi(t.SureTipi), 12),
                new ExcelColumn<IzinTalebiResponse>("Başlangıç", t => t.Baslangic, 17, tarihSaat),
                new ExcelColumn<IzinTalebiResponse>("Bitiş", t => t.Bitis, 17, tarihSaat),
                new ExcelColumn<IzinTalebiResponse>("Süre", t => t.SureMetni, 16),
                new ExcelColumn<IzinTalebiResponse>("Durum", t => DurumAdi(t.Durum), 13),
                new ExcelColumn<IzinTalebiResponse>("Açıklama", t => t.Aciklama, 40),
                new ExcelColumn<IzinTalebiResponse>("Karar Veren", t => t.KararVerenAdi, 24),
                new ExcelColumn<IzinTalebiResponse>("Karar Notu", t => t.KararNotu, 40),
                new ExcelColumn<IzinTalebiResponse>("Talebi Açan", t => t.OlusturanAdi, 24),
                new ExcelColumn<IzinTalebiResponse>("Talep Tarihi", t => sirketSaati.Yerel(t.OlusturmaTarihi), 17, tarihSaat)
            ],
            talepler.Select(t => Tamamla(t, gunluk, adlar)));

        return new DosyaResponse($"izin-talepleri-{sirketSaati.Bugun():yyyyMMdd}.xlsx", ExcelWriter.IcerikTuru, icerik);
    }

    private async Task<int> TalepSahibiAsync(int? istenenCalisanId, KullaniciYetkileri yetkiler, CancellationToken cancellationToken)
    {
        if (istenenCalisanId is null || istenenCalisanId == yetkiler.CalisanId)
        {
            if (yetkiler.CalisanId is not { } kendisi)
            {
                throw new BusinessRuleException("Hesabınız bir çalışan kaydına bağlı değil; talep için çalışan seçin.");
            }

            if (!yetkiler.VarMi(Yetkiler.IzinTalebi.Olustur))
            {
                throw new ForbiddenException("Kendi adınıza izin talebi açma yetkiniz yok.");
            }

            return kendisi;
        }

        if (!yetkiler.VarMi(Yetkiler.IzinTalebi.Yonet))
        {
            throw new ForbiddenException("Başkası adına izin talebi açma yetkiniz yok.");
        }

        var calisan = await kapsam.BulAsync(istenenCalisanId.Value, cancellationToken);
        if (!calisan.Aktif)
        {
            throw new BusinessRuleException("Pasif çalışan adına izin talebi açılamaz.");
        }

        return calisan.Id;
    }

    private async Task<IzinTuru> TurAsync(int izinTuruId, CancellationToken cancellationToken) =>
        await db.IzinTurleri.AsNoTracking().SingleOrDefaultAsync(t => t.Id == izinTuruId, cancellationToken)
        ?? throw new NotFoundException("İzin türü bulunamadı.");

    private async Task<IQueryable<IzinTalebi>> FiltreliAsync(IzinTalebiFiltreRequest request, CancellationToken cancellationToken)
    {
        var calisanlar = await kapsam.CalisanlarAsync(cancellationToken);
        var sorgu = db.IzinTalepleri.AsNoTracking().Where(t => calisanlar.Any(c => c.Id == t.CalisanId));
        if (request.CalisanId is not null)
        {
            sorgu = sorgu.Where(t => t.CalisanId == request.CalisanId);
        }

        if (!string.IsNullOrWhiteSpace(request.CalisanArama))
        {
            var arama = request.CalisanArama.Trim();
            sorgu = sorgu.Where(t => t.Calisan.AdSoyad.Contains(arama));
        }

        if (request.Durum is { Length: > 0 } durumlar)
        {
            sorgu = sorgu.Where(t => durumlar.Contains(t.Durum));
        }

        if (request.IzinTuruId is { Length: > 0 } turler)
        {
            sorgu = sorgu.Where(t => turler.Contains(t.IzinTuruId));
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

        return sorgu;
    }

    private static IQueryable<IzinTalebi> Sirala(IQueryable<IzinTalebi> talepler, string? sort) =>
        talepler.Sirala(sort, SiralamaAlanlari, q => q.OrderByDescending(t => t.Baslangic).ThenByDescending(t => t.Id));

    private static IQueryable<IzinTalebiResponse> Yansit(IQueryable<IzinTalebi> talepler) =>
        talepler.Select(t => new IzinTalebiResponse(
            t.Id, t.CalisanId, t.Calisan.Ad + " " + t.Calisan.Soyad, t.Calisan.Departman.Ad, t.IzinTuruId, t.IzinTuru.Ad,
            t.SureTipi, t.Baslangic, t.Bitis, t.Dakika, string.Empty, t.Durum, t.Aciklama, t.OlusturmaTarihi,
            t.OlusturanKullaniciId, null, t.KararVerenKullaniciId, null, t.KararTarihi, t.KararNotu));

    private static IzinTalebiResponse Tamamla(IzinTalebiResponse talep, int gunluk, IReadOnlyDictionary<int, string> adlar) => talep with
    {
        SureMetni = SureMetni.Olustur(talep.Dakika, gunluk),
        OlusturanAdi = talep.OlusturanKullaniciId is { } olusturan ? adlar.GetValueOrDefault(olusturan) : null,
        KararVerenAdi = talep.KararVerenKullaniciId is { } kararVeren ? adlar.GetValueOrDefault(kararVeren) : null
    };

    private static string BicimAdi(SureTipi tip) => tip switch
    {
        SureTipi.TamGun => "Tam gün",
        SureTipi.YarimGun => "Yarım gün",
        _ => "Saatlik"
    };

    private static string DurumAdi(IzinDurumu durum) => durum switch
    {
        IzinDurumu.Beklemede => "Beklemede",
        IzinDurumu.Onaylandi => "Onaylandı",
        IzinDurumu.Reddedildi => "Reddedildi",
        _ => "İptal edildi"
    };

    private IQueryable<IzinTalebi> AcikTalepler(int calisanId) =>
        db.IzinTalepleri.Where(t => t.CalisanId == calisanId && (t.Durum == IzinDurumu.Beklemede || t.Durum == IzinDurumu.Onaylandi));

    private static SureIstegi Sure(SureHesaplaRequest request) => new(
        request.SureTipi, request.Tarih, request.BitisTarihi, request.GunYarisi, request.BaslangicSaati, request.BitisSaati);
}
