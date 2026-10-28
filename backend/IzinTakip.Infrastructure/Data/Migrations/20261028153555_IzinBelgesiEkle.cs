using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IzinTakip.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class IzinBelgesiEkle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IzinBelgeleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IzinTalebiId = table.Column<int>(type: "int", nullable: false),
                    DosyaAdi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DepoYolu = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IcerikTuru = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Boyut = table.Column<long>(type: "bigint", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OlusturanKullaniciId = table.Column<int>(type: "int", nullable: true),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GuncelleyenKullaniciId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IzinBelgeleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IzinBelgeleri_IzinTalepleri_IzinTalebiId",
                        column: x => x.IzinTalebiId,
                        principalTable: "IzinTalepleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IzinBelgeleri_IzinTalebiId",
                table: "IzinBelgeleri",
                column: "IzinTalebiId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IzinBelgeleri");
        }
    }
}
