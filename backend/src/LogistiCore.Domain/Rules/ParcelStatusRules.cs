using System.Text.RegularExpressions;

namespace LogistiCore.Domain.Rules;

/// <summary>Business rule 2: a parcel may only move along the allowed status path.</summary>
public static class ParcelStatusRules
{
    private static readonly Dictionary<string, string[]> Allowed = new()
    {
        [ParcelStatus.Registered] = new[] { ParcelStatus.AtWarehouse, ParcelStatus.Lost, ParcelStatus.Damaged },
        [ParcelStatus.AtWarehouse] = new[] { ParcelStatus.Sorted, ParcelStatus.Lost, ParcelStatus.Damaged },
        [ParcelStatus.Sorted] = new[] { ParcelStatus.Assigned, ParcelStatus.Lost, ParcelStatus.Damaged },
        [ParcelStatus.Assigned] = new[] { ParcelStatus.OutForDelivery, ParcelStatus.Lost, ParcelStatus.Damaged },
        [ParcelStatus.OutForDelivery] = new[] { ParcelStatus.Delivered, ParcelStatus.FailedAttempt, ParcelStatus.Lost, ParcelStatus.Damaged },
        [ParcelStatus.FailedAttempt] = new[] { ParcelStatus.AtWarehouse, ParcelStatus.Returned, ParcelStatus.Lost, ParcelStatus.Damaged },
    };

    public static bool CanTransition(string from, string to) =>
        Allowed.TryGetValue(from, out var next) && next.Contains(to);

    public static bool IsInWarehouse(string status) =>
        status is ParcelStatus.AtWarehouse or ParcelStatus.Sorted;

    /// <summary>"AtWarehouse" becomes "at warehouse" for use in messages.</summary>
    public static string Label(string status) =>
        Regex.Replace(status, "([a-z])([A-Z])", "$1 $2").ToLowerInvariant();
}
