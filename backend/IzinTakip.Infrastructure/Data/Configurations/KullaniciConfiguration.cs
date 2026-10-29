using IzinTakip.Domain.Entities;
using IzinTakip.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IzinTakip.Infrastructure.Data.Configurations;

public sealed class KullaniciConfiguration : IEntityTypeConfiguration<Kullanici>
{
    public void Configure(EntityTypeBuilder<Kullanici> builder)
    {
        builder.ToTable("Kullanicilar");
        builder.Property(k => k.GeciciParolaBitis).HasConversion<UtcDateTimeConverter>();
        builder.HasIndex(k => k.CalisanId).IsUnique().HasFilter("[CalisanId] IS NOT NULL");
        builder.HasOne<Calisan>().WithMany().HasForeignKey(k => k.CalisanId).OnDelete(DeleteBehavior.Restrict);
    }
}
