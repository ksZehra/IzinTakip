using IzinTakip.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IzinTakip.Infrastructure.Data.Configurations;

public sealed class IzinHakkiKuraliConfiguration : IEntityTypeConfiguration<IzinHakkiKurali>
{
    public void Configure(EntityTypeBuilder<IzinHakkiKurali> builder) => builder.Property(k => k.Ad).HasMaxLength(100);
}
