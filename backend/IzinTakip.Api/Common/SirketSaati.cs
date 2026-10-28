namespace IzinTakip.Api.Common;

public sealed class SirketSaati(TimeProvider timeProvider, TimeZoneInfo saatDilimi)
{
    public DateTime Simdi() => TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), saatDilimi).DateTime;

    public DateOnly Bugun() => DateOnly.FromDateTime(Simdi());
}
