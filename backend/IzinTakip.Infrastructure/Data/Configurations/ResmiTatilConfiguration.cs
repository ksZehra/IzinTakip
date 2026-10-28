using IzinTakip.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IzinTakip.Infrastructure.Data.Configurations;

public sealed class ResmiTatilConfiguration : IEntityTypeConfiguration<ResmiTatil>
{
    public void Configure(EntityTypeBuilder<ResmiTatil> builder)
    {
        builder.Property(r => r.Ad).HasMaxLength(100);
        builder.HasIndex(r => r.Tarih).IsUnique();
    }
}
