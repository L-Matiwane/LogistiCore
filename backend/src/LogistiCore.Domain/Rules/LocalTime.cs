namespace LogistiCore.Domain.Rules;

/// <summary>Everything is stored in UTC. South Africa is UTC+2 all year (no daylight saving).</summary>
public static class LocalTime
{
    public static readonly TimeSpan Offset = TimeSpan.FromHours(2);

    public static DateTime ToLocal(DateTime utc) => utc + Offset;

    public static DateTime StartOfLocalDayUtc(DateTime utc) => ToLocal(utc).Date - Offset;
}
