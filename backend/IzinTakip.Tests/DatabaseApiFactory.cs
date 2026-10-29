namespace IzinTakip.Tests;

public sealed class DatabaseApiFactory : ApiFactory, IAsyncLifetime
{
    private string _connectionString = string.Empty;

    protected override string ConnectionString => _connectionString;

    protected override bool VeritabaniHazirlanir => true;

    public async ValueTask InitializeAsync() => _connectionString = await SqlServerTestContainer.YeniVeritabaniAsync();
}
