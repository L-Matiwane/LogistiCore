namespace LogistiCore.Domain.Rules;

public static class RouteStatus
{
    public const string Planned = "Planned";
    public const string Released = "Released";
    public const string InProgress = "InProgress";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";
}

public static class RouteRules
{
    /// <summary>Parcels can only be added or removed while the route is still being planned.</summary>
    public static bool CanEdit(string status) => status == RouteStatus.Planned;

    public static bool CanRelease(string status) => status == RouteStatus.Planned;

    public static bool CanCancel(string status) => status is RouteStatus.Planned or RouteStatus.Released;

    /// <summary>Business rule 5: the load must not be heavier than the vehicle can carry.</summary>
    public static bool FitsInVehicle(decimal currentLoadKg, decimal addedKg, decimal capacityKg) =>
        currentLoadKg + addedKg <= capacityKg;
}
