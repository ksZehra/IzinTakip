using IzinTakip.Domain.Entities;
using IzinTakip.Infrastructure.Audit;
using IzinTakip.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IzinTakip.Infrastructure.Data;

public sealed class IzinTakipDbContext(DbContextOptions<IzinTakipDbContext> options)
    : IdentityDbContext<Kullanici, Rol, int>(options)
{
    public DbSet<Departman> Departmanlar => Set<Departman>();
    public DbSet<Calisan> Calisanlar => Set<Calisan>();
    public DbSet<IzinTuru> IzinTurleri => Set<IzinTuru>();
    public DbSet<IzinHakkiKurali> IzinHakkiKurallari => Set<IzinHakkiKurali>();
    public DbSet<IzinHakki> IzinHaklari => Set<IzinHakki>();
    public DbSet<IzinTalebi> IzinTalepleri => Set<IzinTalebi>();
    public DbSet<IzinBelgesi> IzinBelgeleri => Set<IzinBelgesi>();
    public DbSet<IslemKaydi> IslemKayitlari => Set<IslemKaydi>();
    public DbSet<Bildirim> Bildirimler => Set<Bildirim>();
    public DbSet<ResmiTatil> ResmiTatiller => Set<ResmiTatil>();
    public DbSet<Ayarlar> Ayarlar => Set<Ayarlar>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<IdentityUserRole<int>>().ToTable("KullaniciRolleri");
        modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("KullaniciClaimleri");
        modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("KullaniciGirisleri");
        modelBuilder.Entity<IdentityUserToken<int>>().ToTable("KullaniciTokenlari");
        modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("RolClaimleri");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IzinTakipDbContext).Assembly);

        foreach (var entityTipi in modelBuilder.Model.GetEntityTypes().Where(tip => tip.ClrType.IsAssignableTo(typeof(Entity))).ToList())
        {
            modelBuilder.Entity(entityTipi.ClrType).Property(nameof(Entity.OlusturmaTarihi)).HasConversion<UtcDateTimeConverter>();
            modelBuilder.Entity(entityTipi.ClrType).Property(nameof(Entity.GuncellemeTarihi)).HasConversion<UtcDateTimeConverter>();
        }
    }
}
