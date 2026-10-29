using System.Globalization;
using System.Text.Json;
using IzinTakip.Domain.Entities;
using IzinTakip.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace IzinTakip.Infrastructure.Audit;

public sealed class AuditLogInterceptor(TimeProvider timeProvider, ICurrentUser gecerliKullanici) : SaveChangesInterceptor
{
    private static readonly IReadOnlyDictionary<Type, Izlenen> Izlenenler = new Dictionary<Type, Izlenen>
    {
        [typeof(Calisan)] = new(nameof(Calisan), null),
        [typeof(IzinHakki)] = new(nameof(IzinHakki), null),
        [typeof(Departman)] = new(nameof(Departman), null),
        [typeof(IzinTuru)] = new(nameof(IzinTuru), null),
        [typeof(IzinHakkiKurali)] = new(nameof(IzinHakkiKurali), null),
        [typeof(ResmiTatil)] = new(nameof(ResmiTatil), null),
        [typeof(Ayarlar)] = new(nameof(Ayarlar), null),
        [typeof(Kullanici)] = new(nameof(Kullanici), [nameof(Kullanici.Email), nameof(Kullanici.CalisanId), nameof(Kullanici.Aktif), nameof(Kullanici.ParolaDegistirmeliMi), nameof(Kullanici.PasswordHash)]),
        [typeof(Rol)] = new(nameof(Rol), [nameof(Rol.Name), nameof(Rol.Korumali)]),
        [typeof(IdentityUserRole<int>)] = new("KullaniciRolu", [nameof(IdentityUserRole<int>.UserId), nameof(IdentityUserRole<int>.RoleId)]),
        [typeof(IdentityRoleClaim<int>)] = new("RolYetkisi", [nameof(IdentityRoleClaim<int>.RoleId), nameof(IdentityRoleClaim<int>.ClaimValue)])
    };

    private static readonly HashSet<string> DenetimAlanlari =
    [
        nameof(Entity.Id), nameof(Entity.OlusturmaTarihi), nameof(Entity.OlusturanKullaniciId), nameof(Entity.GuncellemeTarihi), nameof(Entity.GuncelleyenKullaniciId)
    ];

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        KayitlariEkle(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        KayitlariEkle(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void KayitlariEkle(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var simdi = timeProvider.GetUtcNow().UtcDateTime;
        var kayitlar = context.ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted && Izlenenler.ContainsKey(e.Metadata.ClrType))
            .Select(e => Olustur(context, e, simdi))
            .OfType<IslemKaydi>()
            .ToList();
        context.Set<IslemKaydi>().AddRange(kayitlar);
    }

    private IslemKaydi? Olustur(DbContext context, EntityEntry kayit, DateTime simdi)
    {
        var izlenen = Izlenenler[kayit.Metadata.ClrType];
        var degisiklikler = kayit.Properties
            .Where(p => izlenen.AlanMi(p.Metadata.Name) && (kayit.State != EntityState.Modified || p.IsModified))
            .Select(p => Degisiklik(context, kayit.State, p))
            .OfType<AlanDegisikligi>()
            .Where(d => d.Eski != d.Yeni)
            .ToList();
        if (degisiklikler.Count == 0)
        {
            return null;
        }

        return new IslemKaydi
        {
            Tarih = simdi,
            KullaniciId = gecerliKullanici.KullaniciId,
            Islem = kayit.State switch
            {
                EntityState.Added => "Eklendi",
                EntityState.Modified => "Guncellendi",
                _ => "Silindi"
            },
            Tablo = izlenen.Ad,
            KayitId = kayit.State != EntityState.Added && kayit.Metadata.FindPrimaryKey()?.Properties is [var anahtar]
                ? kayit.Property(anahtar.Name).CurrentValue as int?
                : null,
            IlgiliCalisan = kayit.Entity as Calisan,
            IlgiliCalisanId = kayit.Entity switch
            {
                IzinHakki hak => hak.CalisanId,
                Departman departman => departman.YoneticiId,
                Kullanici kullanici => kullanici.CalisanId,
                _ => null
            },
            IlgiliKullanici = kayit.Entity as Kullanici,
            IlgiliKullaniciId = (kayit.Entity as IdentityUserRole<int>)?.UserId,
            Degisiklikler = JsonSerializer.Serialize(degisiklikler)
        };
    }

    private static AlanDegisikligi? Degisiklik(DbContext context, EntityState durum, PropertyEntry alan)
    {
        if (alan.Metadata.Name == nameof(Kullanici.PasswordHash))
        {
            return durum == EntityState.Modified ? new AlanDegisikligi("Parola", null, "değiştirildi") : null;
        }

        return new AlanDegisikligi(
            alan.Metadata.Name,
            durum == EntityState.Added ? null : Metin(context, alan.Metadata.Name, alan.OriginalValue),
            durum == EntityState.Deleted ? null : Metin(context, alan.Metadata.Name, alan.CurrentValue));
    }

    private static string? Metin(DbContext context, string alan, object? deger) => deger switch
    {
        null => null,
        int rolId when alan == nameof(IdentityUserRole<int>.RoleId) =>
            context.Set<Rol>().Local.FirstOrDefault(r => r.Id == rolId)?.Name ?? rolId.ToString(CultureInfo.InvariantCulture),
        DateOnly tarih => tarih.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        TimeOnly saat => saat.ToString("HH:mm", CultureInfo.InvariantCulture),
        IFormattable bicimli => bicimli.ToString(null, CultureInfo.InvariantCulture),
        _ => deger.ToString()
    };

    private sealed record Izlenen(string Ad, IReadOnlyCollection<string>? Alanlar)
    {
        public bool AlanMi(string alan) => Alanlar?.Contains(alan) ?? !DenetimAlanlari.Contains(alan);
    }
}
