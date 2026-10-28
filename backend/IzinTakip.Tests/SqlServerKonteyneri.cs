using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;

namespace IzinTakip.Tests;

public static class SqlServerKonteyneri
{
    private static readonly Lazy<Task<MsSqlContainer>> Konteyner = new(async () =>
    {
        var konteyner = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-CU27-ubuntu-22.04").Build();
        await konteyner.StartAsync();
        return konteyner;
    });

    public static async Task<string> YeniVeritabaniAsync()
    {
        var konteyner = await Konteyner.Value;
        return new SqlConnectionStringBuilder(konteyner.GetConnectionString())
        {
            InitialCatalog = $"IzinTakip_{Guid.NewGuid():N}"
        }.ConnectionString;
    }
}
