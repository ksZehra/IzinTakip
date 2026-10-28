using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IzinTakip.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CalisanVeTanimlariEkle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "YoneticiId",
                table: "Departmanlar",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Calisanlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Soyad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DepartmanId = table.Column<int>(type: "int", nullable: false),
                    IseGirisTarihi = table.Column<DateOnly>(type: "date", nullable: false),
                    DogumTarihi = table.Column<DateOnly>(type: "date", nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OlusturanKullaniciId = table.Column<int>(type: "int", nullable: true),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GuncelleyenKullaniciId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Calisanlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Calisanlar_Departmanlar_DepartmanId",
                        column: x => x.DepartmanId,
                        principalTable: "Departmanlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IzinHakkiKurallari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MinKidemYil = table.Column<int>(type: "int", nullable: false),
                    MaxKidemYil = table.Column<int>(type: "int", nullable: true),
                    MinYas = table.Column<int>(type: "int", nullable: true),
                    MaxYas = table.Column<int>(type: "int", nullable: true),
                    Gun = table.Column<int>(type: "int", nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OlusturanKullaniciId = table.Column<int>(type: "int", nullable: true),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GuncelleyenKullaniciId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IzinHakkiKurallari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IzinTurleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BakiyedenDuser = table.Column<bool>(type: "bit", nullable: false),
                    BelgeZorunlu = table.Column<bool>(type: "bit", nullable: false),
                    GunHesabi = table.Column<int>(type: "int", nullable: false),
                    TamGunAlinir = table.Column<bool>(type: "bit", nullable: false),
                    YarimGunAlinir = table.Column<bool>(type: "bit", nullable: false),
                    SaatlikAlinir = table.Column<bool>(type: "bit", nullable: false),
                    TalepBasinaEnFazlaDakika = table.Column<int>(type: "int", nullable: true),
                    YillikEnFazlaDakika = table.Column<int>(type: "int", nullable: true),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OlusturanKullaniciId = table.Column<int>(type: "int", nullable: true),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GuncelleyenKullaniciId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IzinTurleri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IzinHaklari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CalisanId = table.Column<int>(type: "int", nullable: false),
                    KazanimTarihi = table.Column<DateOnly>(type: "date", nullable: false),
                    Dakika = table.Column<int>(type: "int", nullable: false),
                    Kaynak = table.Column<int>(type: "int", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OlusturanKullaniciId = table.Column<int>(type: "int", nullable: true),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GuncelleyenKullaniciId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IzinHaklari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IzinHaklari_Calisanlar_CalisanId",
                        column: x => x.CalisanId,
                        principalTable: "Calisanlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Departmanlar_YoneticiId",
                table: "Departmanlar",
                column: "YoneticiId");

            migrationBuilder.CreateIndex(
                name: "IX_Calisanlar_DepartmanId",
                table: "Calisanlar",
                column: "DepartmanId");

            migrationBuilder.CreateIndex(
                name: "IX_IzinHaklari_CalisanId_KazanimTarihi",
                table: "IzinHaklari",
                columns: new[] { "CalisanId", "KazanimTarihi" },
                unique: true,
                filter: "[Kaynak] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_IzinTurleri_Ad",
                table: "IzinTurleri",
                column: "Ad",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Departmanlar_Calisanlar_YoneticiId",
                table: "Departmanlar",
                column: "YoneticiId",
                principalTable: "Calisanlar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Kullanicilar_Calisanlar_CalisanId",
                table: "Kullanicilar",
                column: "CalisanId",
                principalTable: "Calisanlar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departmanlar_Calisanlar_YoneticiId",
                table: "Departmanlar");

            migrationBuilder.DropForeignKey(
                name: "FK_Kullanicilar_Calisanlar_CalisanId",
                table: "Kullanicilar");

            migrationBuilder.DropTable(
                name: "IzinHakkiKurallari");

            migrationBuilder.DropTable(
                name: "IzinHaklari");

            migrationBuilder.DropTable(
                name: "IzinTurleri");

            migrationBuilder.DropTable(
                name: "Calisanlar");

            migrationBuilder.DropIndex(
                name: "IX_Departmanlar_YoneticiId",
                table: "Departmanlar");

            migrationBuilder.DropColumn(
                name: "YoneticiId",
                table: "Departmanlar");
        }
    }
}
