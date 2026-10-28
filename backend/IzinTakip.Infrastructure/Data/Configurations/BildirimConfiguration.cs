using IzinTakip.Domain.Entities;
using IzinTakip.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IzinTakip.Infrastructure.Data.Configurations;

public sealed class BildirimConfiguration : IEntityTypeConfiguration<Bildirim>
{
    public void Configure(EntityTypeBuilder<Bildirim> builder)
    {
        builder.Property(b => b.Baslik).HasMaxLength(200);
        builder.Property(b => b.Metin).HasMaxLength(1000);
        builder.Property(b => b.Baglanti).HasMaxLength(300);
        builder.HasOne<Kullanici>().WithMany().HasForeignKey(b => b.KullaniciId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(b => new { b.KullaniciId, b.OkunduMu });
    }
}
