namespace LogistiCore.Domain.Entities;

public class RateCard
{
    public int RateCardId { get; set; }
    public string ServiceLevel { get; set; } = string.Empty;
    public decimal MinWeightKg { get; set; }
    public decimal MaxWeightKg { get; set; }
    public decimal Price { get; set; }
}
