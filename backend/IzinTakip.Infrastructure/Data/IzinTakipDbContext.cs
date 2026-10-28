using IzinTakip.Domain.Entities;
using IzinTakip.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IzinTakip.Infrastructure.Data;

public sealed class IzinTakipDbContext(DbContextOptions<IzinTakipDbContext> options)
    : IdentityDbContext<Kullanici, Rol, int>(options)
{
    public DbSet<Departman> Departmanlar => Set<Departman>();
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
    }
}
