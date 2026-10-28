using IzinTakip.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IzinTakip.Infrastructure.Data.Configurations;

public sealed class IzinBelgesiConfiguration : IEntityTypeConfiguration<IzinBelgesi>
{
    public void Configure(EntityTypeBuilder<IzinBelgesi> builder)
    {
        builder.Property(b => b.DosyaAdi).HasMaxLength(200);
        builder.Property(b => b.DepoYolu).HasMaxLength(100);
        builder.Property(b => b.IcerikTuru).HasMaxLength(50);
        builder.HasOne<IzinTalebi>().WithMany().HasForeignKey(b => b.IzinTalebiId).OnDelete(DeleteBehavior.Cascade);
    }
}
