namespace LogistiCore.Domain.Entities;

/// <summary>One driver and one vehicle on one day. Stored in the Routes table.</summary>
public class DeliveryRoute
{
    public int RouteId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly RouteDate { get; set; }

    public int DriverId { get; set; }
    public Driver Driver { get; set; } = null!;

    public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;

    public string Status { get; set; } = string.Empty;
    public List<RouteParcel> Parcels { get; set; } = new();
}
