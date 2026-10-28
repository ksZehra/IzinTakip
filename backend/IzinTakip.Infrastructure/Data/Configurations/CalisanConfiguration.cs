using IzinTakip.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IzinTakip.Infrastructure.Data.Configurations;

public sealed class CalisanConfiguration : IEntityTypeConfiguration<Calisan>
{
    public void Configure(EntityTypeBuilder<Calisan> builder)
    {
        builder.Property(c => c.Ad).HasMaxLength(100);
        builder.Property(c => c.Soyad).HasMaxLength(100);
        builder.HasOne(c => c.Departman).WithMany().HasForeignKey(c => c.DepartmanId).OnDelete(DeleteBehavior.Restrict);
    }
}
