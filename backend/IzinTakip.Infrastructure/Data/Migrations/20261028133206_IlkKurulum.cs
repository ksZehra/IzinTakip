using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IzinTakip.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class IlkKurulum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ayarlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    CalismaGunleri = table.Column<int>(type: "int", nullable: false),
                    MesaiBaslangic = table.Column<TimeOnly>(type: "time", nullable: false),
                    MesaiBitis = table.Column<TimeOnly>(type: "time", nullable: false),
                    OgleArasiBaslangic = table.Column<TimeOnly>(type: "time", nullable: false),
                    OgleArasiBitis = table.Column<TimeOnly>(type: "time", nullable: false),
                    SaatlikIzinAdimDakika = table.Column<int>(type: "int", nullable: false),
                    HatirlatmaGunSayisi = table.Column<int>(type: "int", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OlusturanKullaniciId = table.Column<int>(type: "int", nullable: true),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GuncelleyenKullaniciId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ayarlar", x => x.Id);
                    table.CheckConstraint("CK_Ayarlar_TekSatir", "[Id] = 1");
                });

            migrationBuilder.CreateTable(
                name: "Departmanlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OlusturanKullaniciId = table.Column<int>(type: "int", nullable: true),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GuncelleyenKullaniciId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departmanlar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ResmiTatiller",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tarih = table.Column<DateOnly>(type: "date", nullable: false),
                    Ad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    YarimGun = table.Column<bool>(type: "bit", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OlusturanKullaniciId = table.Column<int>(type: "int", nullable: true),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GuncelleyenKullaniciId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResmiTatiller", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Ayarlar",
                columns: new[] { "Id", "CalismaGunleri", "GuncellemeTarihi", "GuncelleyenKullaniciId", "HatirlatmaGunSayisi", "MesaiBaslangic", "MesaiBitis", "OgleArasiBaslangic", "OgleArasiBitis", "OlusturanKullaniciId", "OlusturmaTarihi", "SaatlikIzinAdimDakika" },
                values: new object[] { 1, 31, null, null, 2, new TimeOnly(9, 0, 0), new TimeOnly(18, 0, 0), new TimeOnly(12, 30, 0), new TimeOnly(13, 30, 0), null, new DateTime(2026, 10, 28, 0, 0, 0, 0, DateTimeKind.Utc), 30 });

            migrationBuilder.CreateIndex(
                name: "IX_Departmanlar_Ad",
                table: "Departmanlar",
                column: "Ad",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResmiTatiller_Tarih",
                table: "ResmiTatiller",
                column: "Tarih",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ayarlar");

            migrationBuilder.DropTable(
                name: "Departmanlar");

            migrationBuilder.DropTable(
                name: "ResmiTatiller");
        }
    }
}
