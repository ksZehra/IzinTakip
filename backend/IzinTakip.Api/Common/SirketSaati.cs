namespace IzinTakip.Api.Common;

public sealed class SirketSaati(TimeProvider timeProvider, TimeZoneInfo saatDilimi)
{
    public DateTime Simdi() => TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), saatDilimi).DateTime;

    public DateOnly Bugun() => DateOnly.FromDateTime(Simdi());

    public DateTime Yerel(DateTime utc) => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), saatDilimi);

    public DateTime Utc(DateTime yerel) => TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(yerel, DateTimeKind.Unspecified), saatDilimi);
}
