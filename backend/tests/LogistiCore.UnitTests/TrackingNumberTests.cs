using LogistiCore.Domain.Rules;
using Xunit;

namespace LogistiCore.UnitTests;

public class TrackingNumberTests
{
    [Fact]
    public void Format_uses_the_date_and_a_six_digit_sequence()
    {
        var number = TrackingNumber.Format(new DateTime(2026, 11, 17), 42);
        Assert.Equal("FCL-20261117-000042", number);
    }

    [Fact]
    public void Prefix_ends_with_a_dash()
    {
        Assert.Equal("FCL-20261117-", TrackingNumber.Prefix(new DateTime(2026, 11, 17)));
    }

    [Fact]
    public void Sequence_can_be_read_back_from_a_tracking_number()
    {
        Assert.True(TrackingNumber.TryGetSequence("FCL-20261117-000042", out var sequence));
        Assert.Equal(42, sequence);
    }

    [Fact]
    public void Sequence_cannot_be_read_from_text_that_is_not_a_tracking_number()
    {
        Assert.False(TrackingNumber.TryGetSequence("garbage", out _));
        Assert.False(TrackingNumber.TryGetSequence("FCL-20261117-abc", out _));
    }

    [Fact]
    public void Next_sequence_continues_after_the_last_one()
    {
        TrackingNumber.TryGetSequence("FCL-20261117-000099", out var last);
        var next = TrackingNumber.Format(new DateTime(2026, 11, 17), last + 1);
        Assert.Equal("FCL-20261117-000100", next);
    }
}
