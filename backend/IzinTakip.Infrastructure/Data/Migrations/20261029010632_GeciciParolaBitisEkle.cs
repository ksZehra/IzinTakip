using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IzinTakip.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class GeciciParolaBitisEkle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "GeciciParolaBitis",
                table: "Kullanicilar",
                type: "datetime2",
                nullable: true);

            migrationBuilder.Sql("UPDATE [Kullanicilar] SET [GeciciParolaBitis] = DATEADD(day, 3, SYSUTCDATETIME()) WHERE [ParolaDegistirmeliMi] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GeciciParolaBitis",
                table: "Kullanicilar");
        }
    }
}
