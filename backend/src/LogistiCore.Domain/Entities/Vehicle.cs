namespace LogistiCore.Domain.Entities;

public class Vehicle
{
    public int VehicleId { get; set; }
    public string Registration { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal CapacityKg { get; set; }
    public bool IsActive { get; set; }
}
