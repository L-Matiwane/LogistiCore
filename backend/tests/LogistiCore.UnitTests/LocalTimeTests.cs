using LogistiCore.Domain.Rules;
using Xunit;

namespace LogistiCore.UnitTests;

public class LocalTimeTests
{
    [Fact]
    public void South_African_time_is_two_hours_ahead_of_utc()
    {
        var utc = new DateTime(2026, 11, 17, 10, 0, 0, DateTimeKind.Utc);
        Assert.Equal(new DateTime(2026, 11, 17, 12, 0, 0), LocalTime.ToLocal(utc));
    }

    [Fact]
    public void Start_of_the_local_day_is_returned_in_utc()
    {
        // 23:30 UTC on the 17th is 01:30 on the 18th in South Africa,
        // so the local day started at 22:00 UTC on the 17th.
        var utc = new DateTime(2026, 11, 17, 23, 30, 0, DateTimeKind.Utc);
        Assert.Equal(new DateTime(2026, 11, 17, 22, 0, 0), LocalTime.StartOfLocalDayUtc(utc));
    }
}
