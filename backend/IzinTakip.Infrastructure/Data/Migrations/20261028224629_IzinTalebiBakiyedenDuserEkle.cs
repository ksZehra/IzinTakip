using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IzinTakip.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class IzinTalebiBakiyedenDuserEkle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "BakiyedenDuser",
                table: "IzinTalepleri",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("""
                UPDATE t
                SET BakiyedenDuser = tur.BakiyedenDuser
                FROM IzinTalepleri AS t
                INNER JOIN IzinTurleri AS tur ON tur.Id = t.IzinTuruId;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BakiyedenDuser",
                table: "IzinTalepleri");
        }
    }
}
