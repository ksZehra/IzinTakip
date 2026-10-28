using IzinTakip.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IzinTakip.Infrastructure.Data.Configurations;

public sealed class IzinTuruConfiguration : IEntityTypeConfiguration<IzinTuru>
{
    public void Configure(EntityTypeBuilder<IzinTuru> builder)
    {
        builder.Property(t => t.Ad).HasMaxLength(100);
        builder.HasIndex(t => t.Ad).IsUnique();
    }
}
