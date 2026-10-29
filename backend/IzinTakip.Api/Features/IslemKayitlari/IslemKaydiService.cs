using System.Text.Json;
using IzinTakip.Api.Common;
using IzinTakip.Api.Common.Paging;
using IzinTakip.Infrastructure.Audit;
using IzinTakip.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IzinTakip.Api.Features.IslemKayitlari;

public sealed class IslemKaydiService(IzinTakipDbContext db, KullaniciAdlariProvider kullaniciAdlari, SirketSaati sirketSaati)
{
    public async Task<PagedResponse<IslemKaydiResponse>> ListeleAsync(IslemKaydiListeRequest request, CancellationToken cancellationToken)
    {
        var sorgu = db.IslemKayitlari.AsNoTracking().Select(k => new
        {
            Kayit = k,
            KendiKaydi = k.KullaniciId != null
                         && (k.IlgiliKullaniciId == k.KullaniciId
                             || db.Users.Any(u => u.Id == k.KullaniciId && u.CalisanId != null && u.CalisanId == k.IlgiliCalisanId))
        });

        if (request.KullaniciId is not null)
        {
            sorgu = sorgu.Where(x => x.Kayit.KullaniciId == request.KullaniciId);
        }

        if (request.CalisanId is not null)
        {
            sorgu = sorgu.Where(x => x.Kayit.IlgiliCalisanId == request.CalisanId);
        }

        if (!string.IsNullOrWhiteSpace(request.Tablo))
        {
            sorgu = sorgu.Where(x => x.Kayit.Tablo == request.Tablo);
        }

        if (request.Baslangic is { } baslangic)
        {
            var sinir = sirketSaati.Utc(baslangic.ToDateTime(TimeOnly.MinValue));
            sorgu = sorgu.Where(x => x.Kayit.Tarih >= sinir);
        }

        if (request.Bitis is { } bitis)
        {
            var sinir = sirketSaati.Utc(bitis.AddDays(1).ToDateTime(TimeOnly.MinValue));
            sorgu = sorgu.Where(x => x.Kayit.Tarih < sinir);
        }

        if (request.SadeceKendiKaydi)
        {
            sorgu = sorgu.Where(x => x.KendiKaydi);
        }

        var sayfa = await sorgu
            .OrderByDescending(x => x.Kayit.Id)
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
        var adlar = await kullaniciAdlari.OkuAsync(sayfa.Items.Select(s => s.KullaniciId), cancellationToken);

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
                JsonSerializer.Deserialize<List<AlanDegisikligi>>(s.Degisiklikler) ?? [])).ToList(),
            sayfa.TotalCount,
            sayfa.Page,
            sayfa.PageSize);
    }

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
