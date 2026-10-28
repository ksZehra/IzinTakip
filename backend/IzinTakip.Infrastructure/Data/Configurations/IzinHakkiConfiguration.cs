using IzinTakip.Domain.Entities;
using IzinTakip.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IzinTakip.Infrastructure.Data.Configurations;

public sealed class IzinHakkiConfiguration : IEntityTypeConfiguration<IzinHakki>
{
    public void Configure(EntityTypeBuilder<IzinHakki> builder)
    {
        builder.Property(h => h.Aciklama).HasMaxLength(250);
        builder.HasOne<Calisan>().WithMany().HasForeignKey(h => h.CalisanId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(h => new { h.CalisanId, h.KidemYili })
            .IsUnique()
            .HasFilter($"[Kaynak] = {(int)HakKaynagi.Kural}");
    }
}
