namespace LogistiCore.Domain.Entities;

public class Customer
{
    public int CustomerId { get; set; }
    public string AccountNo { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? BillingAddress { get; set; }
    public bool IsActive { get; set; }
}
