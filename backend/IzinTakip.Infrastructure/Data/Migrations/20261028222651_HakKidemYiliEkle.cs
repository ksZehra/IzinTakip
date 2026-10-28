using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IzinTakip.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class HakKidemYiliEkle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IzinHaklari_CalisanId_KazanimTarihi",
                table: "IzinHaklari");

            migrationBuilder.AddColumn<int>(
                name: "KidemYili",
                table: "IzinHaklari",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HakBaslangicKidemYili",
                table: "Calisanlar",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
                UPDATE c
                SET HakBaslangicKidemYili = CASE WHEN t.TamYil < 0 THEN 1 ELSE t.TamYil + 1 END
                FROM Calisanlar AS c
                CROSS APPLY (
                    SELECT DATEDIFF(year, c.IseGirisTarihi, CAST(c.OlusturmaTarihi AS date))
                           - CASE WHEN DATEADD(year, DATEDIFF(year, c.IseGirisTarihi, CAST(c.OlusturmaTarihi AS date)), c.IseGirisTarihi)
                                       > CAST(c.OlusturmaTarihi AS date) THEN 1 ELSE 0 END AS TamYil
                ) AS t;
                """);

            migrationBuilder.Sql("""
                UPDATE h
                SET KidemYili = DATEDIFF(year, c.IseGirisTarihi, h.KazanimTarihi)
                                - CASE WHEN DATEADD(year, DATEDIFF(year, c.IseGirisTarihi, h.KazanimTarihi), c.IseGirisTarihi)
                                            > h.KazanimTarihi THEN 1 ELSE 0 END
                FROM IzinHaklari AS h
                INNER JOIN Calisanlar AS c ON c.Id = h.CalisanId
                WHERE h.Kaynak = 1;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_IzinHaklari_CalisanId_KidemYili",
                table: "IzinHaklari",
                columns: new[] { "CalisanId", "KidemYili" },
                unique: true,
                filter: "[Kaynak] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IzinHaklari_CalisanId_KidemYili",
                table: "IzinHaklari");

            migrationBuilder.DropColumn(
                name: "KidemYili",
                table: "IzinHaklari");

            migrationBuilder.DropColumn(
                name: "HakBaslangicKidemYili",
                table: "Calisanlar");

            migrationBuilder.CreateIndex(
                name: "IX_IzinHaklari_CalisanId_KazanimTarihi",
                table: "IzinHaklari",
                columns: new[] { "CalisanId", "KazanimTarihi" },
                unique: true,
                filter: "[Kaynak] = 1");
        }
    }
}
