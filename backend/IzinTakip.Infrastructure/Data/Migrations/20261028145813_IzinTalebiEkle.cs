using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IzinTakip.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class IzinTalebiEkle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IzinTalepleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CalisanId = table.Column<int>(type: "int", nullable: false),
                    IzinTuruId = table.Column<int>(type: "int", nullable: false),
                    SureTipi = table.Column<int>(type: "int", nullable: false),
                    Baslangic = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Bitis = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Dakika = table.Column<int>(type: "int", nullable: false),
                    Durum = table.Column<int>(type: "int", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    KararVerenKullaniciId = table.Column<int>(type: "int", nullable: true),
                    KararTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    KararNotu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OlusturanKullaniciId = table.Column<int>(type: "int", nullable: true),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GuncelleyenKullaniciId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IzinTalepleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IzinTalepleri_Calisanlar_CalisanId",
                        column: x => x.CalisanId,
                        principalTable: "Calisanlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IzinTalepleri_IzinTurleri_IzinTuruId",
                        column: x => x.IzinTuruId,
                        principalTable: "IzinTurleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IzinTalepleri_CalisanId_Baslangic",
                table: "IzinTalepleri",
                columns: new[] { "CalisanId", "Baslangic" });

            migrationBuilder.CreateIndex(
                name: "IX_IzinTalepleri_Durum_Baslangic",
                table: "IzinTalepleri",
                columns: new[] { "Durum", "Baslangic" });

            migrationBuilder.CreateIndex(
                name: "IX_IzinTalepleri_IzinTuruId",
                table: "IzinTalepleri",
                column: "IzinTuruId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IzinTalepleri");
        }
    }
}
