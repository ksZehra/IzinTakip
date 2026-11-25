using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IzinTakip.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CalisanAdSoyadKolonu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdSoyad",
                table: "Calisanlar",
                type: "nvarchar(201)",
                maxLength: 201,
                nullable: false,
                computedColumnSql: "[Ad] + N' ' + [Soyad]",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_Calisanlar_AdSoyad",
                table: "Calisanlar",
                column: "AdSoyad");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Calisanlar_AdSoyad",
                table: "Calisanlar");

            migrationBuilder.DropColumn(
                name: "AdSoyad",
                table: "Calisanlar");
        }
    }
}
