namespace IzinTakip.Api.BackgroundServices;

public sealed class BackgroundJobOptions
{
    public TimeOnly YillikHakSaati { get; init; } = new(0, 10);
    public TimeOnly HatirlatmaSaati { get; init; } = new(8, 0);
}
