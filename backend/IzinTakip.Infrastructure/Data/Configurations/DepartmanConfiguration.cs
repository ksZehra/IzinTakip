using IzinTakip.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IzinTakip.Infrastructure.Data.Configurations;

public sealed class DepartmanConfiguration : IEntityTypeConfiguration<Departman>
{
    public void Configure(EntityTypeBuilder<Departman> builder)
    {
        builder.Property(d => d.Ad).HasMaxLength(100);
        builder.HasIndex(d => d.Ad).IsUnique();
    }
}
