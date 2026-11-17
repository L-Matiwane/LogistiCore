namespace LogistiCore.Domain.Entities;

public class WarehouseBin
{
    public int BinId { get; set; }
    public string Zone { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int Capacity { get; set; }
}
