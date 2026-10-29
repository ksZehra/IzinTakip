using IzinTakip.Infrastructure.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IzinTakip.Infrastructure.Data.Configurations;

public sealed class IslemKaydiConfiguration : IEntityTypeConfiguration<IslemKaydi>
{
    public void Configure(EntityTypeBuilder<IslemKaydi> builder)
    {
        builder.Property(k => k.Islem).HasMaxLength(20);
        builder.Property(k => k.Tablo).HasMaxLength(50);
        builder.Property(k => k.Tarih).HasConversion<UtcDateTimeConverter>();
        builder.HasOne(k => k.IlgiliCalisan).WithMany().HasForeignKey(k => k.IlgiliCalisanId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(k => k.IlgiliKullanici).WithMany().HasForeignKey(k => k.IlgiliKullaniciId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(k => k.KullaniciId);
        builder.HasIndex(k => k.Tarih);
    }
}
