namespace IzinTakip.Api.BackgroundServices;

public sealed class ArkaPlanAyarlari
{
    public TimeOnly YillikHakSaati { get; init; } = new(0, 10);
    public TimeOnly HatirlatmaSaati { get; init; } = new(8, 0);
}
