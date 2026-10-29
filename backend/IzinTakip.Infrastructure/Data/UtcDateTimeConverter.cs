using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace IzinTakip.Infrastructure.Data;

public sealed class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
    zaman => zaman,
    zaman => DateTime.SpecifyKind(zaman, DateTimeKind.Utc));
