using LogistiCore.Domain.Rules;

namespace LogistiCore.Application.Dispatch;

public record DriverDto(int DriverId, string FullName, string LicenceNo, string? Phone, bool IsAvailable);

public record VehicleDto(int VehicleId, string Registration, string? Description, decimal CapacityKg, bool IsActive);

public record DispatchParcelDto(
    int ParcelId,
    string TrackingNo,
    string RecipientName,
    string City,
    string ServiceLevel,
    decimal WeightKg,
    string? BinCode,
    string[] SpecialHandling);

public record RouteParcelDto(
    int ParcelId,
    int StopSequence,
    string TrackingNo,
    string RecipientName,
    string DeliveryAddress,
    string City,
    string ServiceLevel,
    decimal WeightKg,
    string CurrentStatus);

public record RouteDto(
    int RouteId,
    string Name,
    DateOnly RouteDate,
    int DriverId,
    string DriverName,
    int VehicleId,
    string VehicleRegistration,
    decimal CapacityKg,
    decimal LoadKg,
    string Status,
    IReadOnlyList<RouteParcelDto> Parcels);

public record CreateRouteRequest(string Name, DateOnly RouteDate, int DriverId, int VehicleId);
public record AddParcelsRequest(int[]? ParcelIds);
public record SaveVehicleRequest(string Registration, string? Description, decimal CapacityKg, bool IsActive);
public record AvailabilityRequest(bool IsAvailable);

public interface IDispatchService
{
    Task<IReadOnlyList<DriverDto>> ListDriversAsync(CancellationToken ct = default);
    Task<DriverDto> SetDriverAvailabilityAsync(int driverId, bool isAvailable, CancellationToken ct = default);

    Task<IReadOnlyList<VehicleDto>> ListVehiclesAsync(CancellationToken ct = default);
    Task<VehicleDto> CreateVehicleAsync(SaveVehicleRequest request, CancellationToken ct = default);
    Task<VehicleDto> UpdateVehicleAsync(int vehicleId, SaveVehicleRequest request, CancellationToken ct = default);

    Task<IReadOnlyList<DispatchParcelDto>> ListUnassignedParcelsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<RouteDto>> ListRoutesAsync(DateOnly date, CancellationToken ct = default);
    Task<RouteDto> CreateRouteAsync(CreateRouteRequest request, CancellationToken ct = default);
    Task<RouteDto> AddParcelsAsync(int routeId, AddParcelsRequest request, int userId, CancellationToken ct = default);
    Task<RouteDto> RemoveParcelAsync(int routeId, int parcelId, int userId, CancellationToken ct = default);
    Task<RouteDto> ReleaseRouteAsync(int routeId, CancellationToken ct = default);
    Task<RouteDto> CancelRouteAsync(int routeId, int userId, CancellationToken ct = default);
}

public static class DispatchValidation
{
    public static Dictionary<string, string[]> ValidateCreateRoute(CreateRouteRequest r)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(r.Name))
        {
            errors["name"] = new[] { "Give the route a name, for example Route A - Beacon Bay." };
        }
        else if (r.Name.Trim().Length > 100)
        {
            errors["name"] = new[] { "The route name must be 100 characters or fewer." };
        }

        var today = DateOnly.FromDateTime(LocalTime.ToLocal(DateTime.UtcNow));
        if (r.RouteDate < today)
        {
            errors["routeDate"] = new[] { "A route cannot be planned for a day that has already passed." };
        }

        if (r.DriverId <= 0)
        {
            errors["driverId"] = new[] { "Choose a driver." };
        }

        if (r.VehicleId <= 0)
        {
            errors["vehicleId"] = new[] { "Choose a vehicle." };
        }

        return errors;
    }

    public static Dictionary<string, string[]> ValidateAddParcels(AddParcelsRequest r)
    {
        var errors = new Dictionary<string, string[]>();

        if (r.ParcelIds is null || r.ParcelIds.Length == 0)
        {
            errors["parcelIds"] = new[] { "Select at least one parcel." };
        }
        else if (r.ParcelIds.Length > 100)
        {
            errors["parcelIds"] = new[] { "Add at most 100 parcels at a time." };
        }
        else if (r.ParcelIds.Any(id => id <= 0))
        {
            errors["parcelIds"] = new[] { "One of the selected parcels is not valid." };
        }

        return errors;
    }

    public static Dictionary<string, string[]> ValidateVehicle(SaveVehicleRequest r)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(r.Registration))
        {
            errors["registration"] = new[] { "Registration number is required." };
        }
        else if (r.Registration.Trim().Length > 20)
        {
            errors["registration"] = new[] { "Registration must be 20 characters or fewer." };
        }

        if (r.CapacityKg <= 0 || r.CapacityKg > 20000)
        {
            errors["capacityKg"] = new[] { "Capacity must be between 1 and 20 000 kg." };
        }

        if (r.Description is { Length: > 100 })
        {
            errors["description"] = new[] { "Description must be 100 characters or fewer." };
        }

        return errors;
    }
}
