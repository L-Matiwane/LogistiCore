namespace LogistiCore.Domain.Entities;

public class ParcelStatusHistory
{
    public long HistoryId { get; set; }
    public int ParcelId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? ChangedByUserId { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? Note { get; set; }
}
