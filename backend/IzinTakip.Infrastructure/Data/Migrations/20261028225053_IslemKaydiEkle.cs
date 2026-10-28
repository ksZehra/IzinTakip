using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IzinTakip.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class IslemKaydiEkle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IslemKayitlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false),
                    KullaniciId = table.Column<int>(type: "int", nullable: true),
                    Islem = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Tablo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    KayitId = table.Column<int>(type: "int", nullable: true),
                    IlgiliCalisanId = table.Column<int>(type: "int", nullable: true),
                    IlgiliKullaniciId = table.Column<int>(type: "int", nullable: true),
                    Degisiklikler = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IslemKayitlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IslemKayitlari_Calisanlar_IlgiliCalisanId",
                        column: x => x.IlgiliCalisanId,
                        principalTable: "Calisanlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IslemKayitlari_Kullanicilar_IlgiliKullaniciId",
                        column: x => x.IlgiliKullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IslemKayitlari_IlgiliCalisanId",
                table: "IslemKayitlari",
                column: "IlgiliCalisanId");

            migrationBuilder.CreateIndex(
                name: "IX_IslemKayitlari_IlgiliKullaniciId",
                table: "IslemKayitlari",
                column: "IlgiliKullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_IslemKayitlari_KullaniciId",
                table: "IslemKayitlari",
                column: "KullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_IslemKayitlari_Tarih",
                table: "IslemKayitlari",
                column: "Tarih");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IslemKayitlari");
        }
    }
}
