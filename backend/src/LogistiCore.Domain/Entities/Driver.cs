namespace LogistiCore.Domain.Entities;

public class Driver
{
    public int DriverId { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public string LicenceNo { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public bool IsAvailable { get; set; }
}
