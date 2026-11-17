namespace LogistiCore.Domain.Entities;

public class ParcelException
{
    public int ExceptionId { get; set; }
    public int ParcelId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public int ReportedByUserId { get; set; }
    public DateTime ReportedAt { get; set; }
    public bool Resolved { get; set; }
}
