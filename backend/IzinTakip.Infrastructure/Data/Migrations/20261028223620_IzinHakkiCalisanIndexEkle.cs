using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IzinTakip.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class IzinHakkiCalisanIndexEkle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_IzinHaklari_CalisanId",
                table: "IzinHaklari",
                column: "CalisanId")
                .Annotation("SqlServer:Include", new[] { "Dakika" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IzinHaklari_CalisanId",
                table: "IzinHaklari");
        }
    }
}
