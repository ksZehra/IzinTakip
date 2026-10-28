namespace IzinTakip.Infrastructure.Data.Seed;

public sealed record BaslangicVerisiAyarlari(
    string? SuperAdminEposta,
    string? SuperAdminParola,
    bool DemoVeri,
    string? DemoParola);
