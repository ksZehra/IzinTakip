using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace IzinTakip.Infrastructure.Data;

public sealed class UtcZamanDonusturucu() : ValueConverter<DateTime, DateTime>(
    zaman => zaman,
    zaman => DateTime.SpecifyKind(zaman, DateTimeKind.Utc));
