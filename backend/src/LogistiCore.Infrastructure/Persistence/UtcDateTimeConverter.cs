using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LogistiCore.Infrastructure.Persistence;

/// <summary>SQL Server returns dates with no time zone. Everything we store is UTC, so mark it as UTC when reading.</summary>
public sealed class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter()
        : base(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
    {
    }
}
