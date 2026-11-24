namespace LogistiCore.Domain.Entities;

public class RouteParcel
{
    public int RouteParcelId { get; set; }
    public int RouteId { get; set; }
    public int ParcelId { get; set; }
    public Parcel Parcel { get; set; } = null!;
    public int StopSequence { get; set; }
    public bool IsActive { get; set; }
}
