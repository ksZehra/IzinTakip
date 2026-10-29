namespace IzinTakip.Infrastructure.Data.Seed;

public sealed record SeedOptions(
    string? SuperAdminEposta,
    string? SuperAdminParola,
    bool DemoVeri,
    string? DemoParola);
