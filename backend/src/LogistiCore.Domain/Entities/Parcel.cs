namespace LogistiCore.Domain.Entities;

public class Parcel
{
    public int ParcelId { get; set; }
    public string TrackingNo { get; set; } = string.Empty;

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public string RecipientName { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public string? RecipientEmail { get; set; }
    public string DeliveryAddress { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string? DeliveryInstructions { get; set; }
    public string? SpecialHandling { get; set; }

    public decimal WeightKg { get; set; }
    public decimal? LengthCm { get; set; }
    public decimal? WidthCm { get; set; }
    public decimal? HeightCm { get; set; }
    public decimal BilledWeightKg { get; set; }

    public string ServiceLevel { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string CurrentStatus { get; set; } = string.Empty;
    public bool IsInvoiced { get; set; }

    public int? BinId { get; set; }
    public WarehouseBin? Bin { get; set; }

    public int RegisteredByUserId { get; set; }
    public DateTime RegisteredAt { get; set; }

    public List<ParcelStatusHistory> History { get; set; } = new();
}
