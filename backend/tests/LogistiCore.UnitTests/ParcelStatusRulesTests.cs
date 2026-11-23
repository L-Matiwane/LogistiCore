using LogistiCore.Domain.Rules;
using Xunit;

namespace LogistiCore.UnitTests;

public class ParcelStatusRulesTests
{
    [Theory]
    [InlineData("Registered", "AtWarehouse")]
    [InlineData("AtWarehouse", "Sorted")]
    [InlineData("Sorted", "Assigned")]
    [InlineData("Assigned", "OutForDelivery")]
    [InlineData("OutForDelivery", "Delivered")]
    [InlineData("OutForDelivery", "FailedAttempt")]
    [InlineData("FailedAttempt", "AtWarehouse")]
    [InlineData("FailedAttempt", "Returned")]
    [InlineData("Registered", "Lost")]
    [InlineData("AtWarehouse", "Damaged")]
    [InlineData("Sorted", "Lost")]
    [InlineData("OutForDelivery", "Damaged")]
    public void Allowed_status_changes_are_accepted(string from, string to)
    {
        Assert.True(ParcelStatusRules.CanTransition(from, to));
    }

    [Theory]
    [InlineData("Registered", "Sorted")]
    [InlineData("Registered", "Delivered")]
    [InlineData("AtWarehouse", "Registered")]
    [InlineData("AtWarehouse", "AtWarehouse")]
    [InlineData("Sorted", "AtWarehouse")]
    [InlineData("Delivered", "AtWarehouse")]
    [InlineData("Delivered", "Lost")]
    [InlineData("Returned", "AtWarehouse")]
    [InlineData("Lost", "AtWarehouse")]
    [InlineData("Damaged", "Lost")]
    public void Status_changes_that_break_the_path_are_rejected(string from, string to)
    {
        Assert.False(ParcelStatusRules.CanTransition(from, to));
    }

    [Fact]
    public void Unknown_status_cannot_change()
    {
        Assert.False(ParcelStatusRules.CanTransition("Nonsense", "Delivered"));
    }

    [Theory]
    [InlineData("AtWarehouse", true)]
    [InlineData("Sorted", true)]
    [InlineData("Registered", false)]
    [InlineData("OutForDelivery", false)]
    [InlineData("Delivered", false)]
    public void Warehouse_stock_is_at_warehouse_or_sorted(string status, bool expected)
    {
        Assert.Equal(expected, ParcelStatusRules.IsInWarehouse(status));
    }

    [Fact]
    public void Label_turns_a_status_into_readable_words()
    {
        Assert.Equal("at warehouse", ParcelStatusRules.Label("AtWarehouse"));
        Assert.Equal("out for delivery", ParcelStatusRules.Label("OutForDelivery"));
        Assert.Equal("lost", ParcelStatusRules.Label("Lost"));
    }
}
