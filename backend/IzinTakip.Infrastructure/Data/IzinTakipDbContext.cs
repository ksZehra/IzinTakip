using IzinTakip.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IzinTakip.Infrastructure.Data;

public sealed class IzinTakipDbContext(DbContextOptions<IzinTakipDbContext> options) : DbContext(options)
{
    public DbSet<Departman> Departmanlar => Set<Departman>();
    public DbSet<ResmiTatil> ResmiTatiller => Set<ResmiTatil>();
    public DbSet<Ayarlar> Ayarlar => Set<Ayarlar>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IzinTakipDbContext).Assembly);
}
