using IzinTakip.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IzinTakip.Infrastructure.Data.Configurations;

public sealed class IzinTalebiConfiguration : IEntityTypeConfiguration<IzinTalebi>
{
    public void Configure(EntityTypeBuilder<IzinTalebi> builder)
    {
        builder.Property(t => t.Aciklama).HasMaxLength(500);
        builder.Property(t => t.KararNotu).HasMaxLength(500);
        builder.Property(t => t.RowVersion).IsRowVersion();
        builder.HasOne(t => t.Calisan).WithMany().HasForeignKey(t => t.CalisanId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(t => t.IzinTuru).WithMany().HasForeignKey(t => t.IzinTuruId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(t => new { t.CalisanId, t.Baslangic });
        builder.HasIndex(t => new { t.Durum, t.Baslangic });
    }
}
