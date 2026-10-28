using IzinTakip.Domain.Entities;
using IzinTakip.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IzinTakip.Infrastructure.Data.Configurations;

public sealed class AyarlarConfiguration : IEntityTypeConfiguration<Ayarlar>
{
    public void Configure(EntityTypeBuilder<Ayarlar> builder)
    {
        builder.ToTable(t => t.HasCheckConstraint("CK_Ayarlar_TekSatir", $"[Id] = {Ayarlar.TekSatirId}"));
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.HasData(new
        {
            Id = Ayarlar.TekSatirId,
            CalismaGunleri = HaftaGunleri.Pazartesi | HaftaGunleri.Sali | HaftaGunleri.Carsamba | HaftaGunleri.Persembe | HaftaGunleri.Cuma,
            MesaiBaslangic = new TimeOnly(9, 0),
            MesaiBitis = new TimeOnly(18, 0),
            OgleArasiBaslangic = new TimeOnly(12, 30),
            OgleArasiBitis = new TimeOnly(13, 30),
            SaatlikIzinAdimDakika = 30,
            HatirlatmaGunSayisi = 2,
            OlusturmaTarihi = new DateTime(2026, 10, 28, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
