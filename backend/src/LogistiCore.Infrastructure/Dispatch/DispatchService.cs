using LogistiCore.Application.Dispatch;
using LogistiCore.Domain.Entities;
using LogistiCore.Domain.Rules;
using LogistiCore.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LogistiCore.Infrastructure.Dispatch;

public class DispatchService : IDispatchService
{
    private readonly AppDbContext _db;

    public DispatchService(AppDbContext db) => _db = db;

    // ------------------------------------------------------------ drivers and vehicles

    public async Task<IReadOnlyList<DriverDto>> ListDriversAsync(CancellationToken ct = default)
    {
        var drivers = await _db.Drivers.AsNoTracking().Include(d => d.User).OrderBy(d => d.User.FullName).ToListAsync(ct);
        return drivers.Select(ToDto).ToList();
    }

    public async Task<DriverDto> SetDriverAvailabilityAsync(int driverId, bool isAvailable, CancellationToken ct = default)
    {
        var driver = await _db.Drivers.Include(d => d.User).FirstOrDefaultAsync(d => d.DriverId == driverId, ct)
            ?? throw new BusinessRuleException("That driver does not exist.");

        driver.IsAvailable = isAvailable;
        await _db.SaveChangesAsync(ct);
        return ToDto(driver);
    }

    public async Task<IReadOnlyList<VehicleDto>> ListVehiclesAsync(CancellationToken ct = default)
    {
        var vehicles = await _db.Vehicles.AsNoTracking().OrderBy(v => v.Registration).ToListAsync(ct);
        return vehicles.Select(ToDto).ToList();
    }

    public async Task<VehicleDto> CreateVehicleAsync(SaveVehicleRequest request, CancellationToken ct = default)
    {
        var registration = request.Registration.Trim().ToUpperInvariant();
        if (await _db.Vehicles.AnyAsync(v => v.Registration == registration, ct))
        {
            throw new BusinessRuleException($"A vehicle with registration {registration} already exists.");
        }

        var vehicle = new Vehicle
        {
            Registration = registration,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            CapacityKg = request.CapacityKg,
            IsActive = request.IsActive
        };

        _db.Vehicles.Add(vehicle);
        await SaveAsync(ct, $"A vehicle with registration {registration} already exists.");
        return ToDto(vehicle);
    }

    public async Task<VehicleDto> UpdateVehicleAsync(int vehicleId, SaveVehicleRequest request, CancellationToken ct = default)
    {
        var vehicle = await _db.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == vehicleId, ct)
            ?? throw new BusinessRuleException("That vehicle does not exist.");

        var registration = request.Registration.Trim().ToUpperInvariant();
        if (await _db.Vehicles.AnyAsync(v => v.Registration == registration && v.VehicleId != vehicleId, ct))
        {
            throw new BusinessRuleException($"A vehicle with registration {registration} already exists.");
        }

        if (!request.IsActive && vehicle.IsActive)
        {
            var inUse = await _db.DeliveryRoutes.AnyAsync(
                r => r.VehicleId == vehicleId && (r.Status == RouteStatus.Planned || r.Status == RouteStatus.Released), ct);
            if (inUse)
            {
                throw new BusinessRuleException("This vehicle is on a planned or released route. Cancel or finish the route first.");
            }
        }

        vehicle.Registration = registration;
        vehicle.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        vehicle.CapacityKg = request.CapacityKg;
        vehicle.IsActive = request.IsActive;
        await SaveAsync(ct, $"A vehicle with registration {registration} already exists.");
        return ToDto(vehicle);
    }

    // ------------------------------------------------------------ routes

    public async Task<IReadOnlyList<DispatchParcelDto>> ListUnassignedParcelsAsync(CancellationToken ct = default)
    {
        var rows = await _db.Parcels
            .AsNoTracking()
            .Where(p => p.CurrentStatus == ParcelStatus.Sorted)
            .OrderBy(p => p.RegisteredAt)
            .Take(300)
            .Select(p => new
            {
                p.ParcelId,
                p.TrackingNo,
                p.RecipientName,
                p.City,
                p.ServiceLevel,
                p.WeightKg,
                BinCode = p.Bin == null ? null : p.Bin.Code,
                p.SpecialHandling
            })
            .ToListAsync(ct);

        return rows
            .Select(r => new DispatchParcelDto(
                r.ParcelId,
                r.TrackingNo,
                r.RecipientName,
                r.City,
                r.ServiceLevel,
                r.WeightKg,
                r.BinCode,
                string.IsNullOrWhiteSpace(r.SpecialHandling)
                    ? Array.Empty<string>()
                    : r.SpecialHandling.Split(',', StringSplitOptions.RemoveEmptyEntries)))
            .ToList();
    }

    public async Task<IReadOnlyList<RouteDto>> ListRoutesAsync(DateOnly date, CancellationToken ct = default)
    {
        var routes = await RoutesWithDetails()
            .AsNoTracking()
            .Where(r => r.RouteDate == date && r.Status != RouteStatus.Cancelled)
            .OrderBy(r => r.Name)
            .ToListAsync(ct);

        return routes.Select(ToDto).ToList();
    }

    public async Task<RouteDto> CreateRouteAsync(CreateRouteRequest request, CancellationToken ct = default)
    {
        var driver = await _db.Drivers.Include(d => d.User).FirstOrDefaultAsync(d => d.DriverId == request.DriverId, ct)
            ?? throw new BusinessRuleException("That driver does not exist.");

        if (!driver.IsAvailable)
        {
            throw new BusinessRuleException($"{driver.User.FullName} is marked as unavailable.");
        }

        var vehicle = await _db.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == request.VehicleId, ct)
            ?? throw new BusinessRuleException("That vehicle does not exist.");

        if (!vehicle.IsActive)
        {
            throw new BusinessRuleException($"Vehicle {vehicle.Registration} is not in service.");
        }

        var dayText = request.RouteDate.ToString("dd MMM yyyy");
        var driverMessage = $"{driver.User.FullName} already has a route on {dayText}.";
        var vehicleMessage = $"Vehicle {vehicle.Registration} is already on a route on {dayText}.";

        if (await _db.DeliveryRoutes.AnyAsync(
                r => r.DriverId == driver.DriverId && r.RouteDate == request.RouteDate && r.Status != RouteStatus.Cancelled, ct))
        {
            throw new BusinessRuleException(driverMessage);
        }

        if (await _db.DeliveryRoutes.AnyAsync(
                r => r.VehicleId == vehicle.VehicleId && r.RouteDate == request.RouteDate && r.Status != RouteStatus.Cancelled, ct))
        {
            throw new BusinessRuleException(vehicleMessage);
        }

        var route = new DeliveryRoute
        {
            Name = request.Name.Trim(),
            RouteDate = request.RouteDate,
            DriverId = driver.DriverId,
            VehicleId = vehicle.VehicleId,
            Status = RouteStatus.Planned
        };

        _db.DeliveryRoutes.Add(route);
        await SaveAsync(ct, "That driver or vehicle was just booked on this day. Refresh and try again.");
        return await LoadRouteAsync(route.RouteId, ct);
    }

    public async Task<RouteDto> AddParcelsAsync(int routeId, AddParcelsRequest request, int userId, CancellationToken ct = default)
    {
        var route = await RoutesWithDetails().FirstOrDefaultAsync(r => r.RouteId == routeId, ct)
            ?? throw new BusinessRuleException("That route does not exist.");

        if (!RouteRules.CanEdit(route.Status))
        {
            throw new BusinessRuleException(
                $"Parcels can only be added while a route is planned. This route is {route.Status.ToLowerInvariant()}.");
        }

        var ids = (request.ParcelIds ?? Array.Empty<int>()).Distinct().ToList();
        var parcels = await _db.Parcels.Where(p => ids.Contains(p.ParcelId)).ToListAsync(ct);

        if (parcels.Count != ids.Count)
        {
            throw new BusinessRuleException("One or more of the selected parcels could not be found.");
        }

        foreach (var parcel in parcels)
        {
            if (parcel.CurrentStatus != ParcelStatus.Sorted)
            {
                throw new BusinessRuleException(
                    $"{parcel.TrackingNo} is {ParcelStatusRules.Label(parcel.CurrentStatus)}. Only sorted parcels can be put on a route.");
            }
        }

        var currentLoad = route.Parcels.Sum(rp => rp.Parcel.WeightKg);
        var addedLoad = parcels.Sum(p => p.WeightKg);

        if (!RouteRules.FitsInVehicle(currentLoad, addedLoad, route.Vehicle.CapacityKg))
        {
            throw new BusinessRuleException(
                $"That would load {currentLoad + addedLoad:0.##} kg onto {route.Vehicle.Registration}, which carries {route.Vehicle.CapacityKg:0.##} kg.");
        }

        var lastStop = await _db.RouteParcels
            .Where(rp => rp.RouteId == routeId)
            .MaxAsync(rp => (int?)rp.StopSequence, ct) ?? 0;

        foreach (var id in ids)
        {
            var parcel = parcels.First(p => p.ParcelId == id);
            _db.RouteParcels.Add(new RouteParcel { RouteId = routeId, ParcelId = parcel.ParcelId, StopSequence = ++lastStop, IsActive = true });
            parcel.CurrentStatus = ParcelStatus.Assigned;
            AddHistory(parcel.ParcelId, ParcelStatus.Assigned, userId, $"Assigned to route {route.Name}");
        }

        await SaveAsync(ct, "One of those parcels was just put on another route. Refresh and try again.");
        return await LoadRouteAsync(routeId, ct);
    }

    public async Task<RouteDto> RemoveParcelAsync(int routeId, int parcelId, int userId, CancellationToken ct = default)
    {
        var route = await RoutesWithDetails().FirstOrDefaultAsync(r => r.RouteId == routeId, ct)
            ?? throw new BusinessRuleException("That route does not exist.");

        if (!RouteRules.CanEdit(route.Status))
        {
            throw new BusinessRuleException(
                $"Parcels can only be removed while a route is planned. This route is {route.Status.ToLowerInvariant()}.");
        }

        var stop = route.Parcels.FirstOrDefault(rp => rp.ParcelId == parcelId)
            ?? throw new BusinessRuleException("That parcel is not on this route.");

        stop.IsActive = false;
        stop.Parcel.CurrentStatus = ParcelStatus.Sorted;
        AddHistory(parcelId, ParcelStatus.Sorted, userId, $"Removed from route {route.Name}");

        await _db.SaveChangesAsync(ct);
        return await LoadRouteAsync(routeId, ct);
    }

    public async Task<RouteDto> ReleaseRouteAsync(int routeId, CancellationToken ct = default)
    {
        var route = await RoutesWithDetails().FirstOrDefaultAsync(r => r.RouteId == routeId, ct)
            ?? throw new BusinessRuleException("That route does not exist.");

        if (!RouteRules.CanRelease(route.Status))
        {
            throw new BusinessRuleException($"This route is already {route.Status.ToLowerInvariant()}.");
        }

        if (route.Parcels.Count == 0)
        {
            throw new BusinessRuleException("Add at least one parcel before releasing the route.");
        }

        route.Status = RouteStatus.Released;
        await _db.SaveChangesAsync(ct);
        return await LoadRouteAsync(routeId, ct);
    }

    public async Task<RouteDto> CancelRouteAsync(int routeId, int userId, CancellationToken ct = default)
    {
        var route = await RoutesWithDetails().FirstOrDefaultAsync(r => r.RouteId == routeId, ct)
            ?? throw new BusinessRuleException("That route does not exist.");

        if (!RouteRules.CanCancel(route.Status))
        {
            throw new BusinessRuleException($"A route that is {route.Status.ToLowerInvariant()} cannot be cancelled.");
        }

        foreach (var stop in route.Parcels)
        {
            stop.IsActive = false;
            if (stop.Parcel.CurrentStatus == ParcelStatus.Assigned)
            {
                stop.Parcel.CurrentStatus = ParcelStatus.Sorted;
                AddHistory(stop.ParcelId, ParcelStatus.Sorted, userId, $"Route {route.Name} was cancelled");
            }
        }

        route.Status = RouteStatus.Cancelled;
        await _db.SaveChangesAsync(ct);
        return await LoadRouteAsync(routeId, ct);
    }

    // ------------------------------------------------------------ helpers

    private IQueryable<DeliveryRoute> RoutesWithDetails() =>
        _db.DeliveryRoutes
            .Include(r => r.Driver).ThenInclude(d => d.User)
            .Include(r => r.Vehicle)
            .Include(r => r.Parcels.Where(p => p.IsActive)).ThenInclude(p => p.Parcel);

    private async Task<RouteDto> LoadRouteAsync(int routeId, CancellationToken ct)
    {
        var route = await RoutesWithDetails().AsNoTracking().FirstAsync(r => r.RouteId == routeId, ct);
        return ToDto(route);
    }

    private void AddHistory(int parcelId, string status, int userId, string note) =>
        _db.StatusHistory.Add(new ParcelStatusHistory
        {
            ParcelId = parcelId,
            Status = status,
            ChangedByUserId = userId,
            ChangedAt = DateTime.UtcNow,
            Note = note
        });

    /// <summary>Saves, and turns a unique-index clash (two people at once) into a clear message.</summary>
    private async Task SaveAsync(CancellationToken ct, string duplicateMessage)
    {
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw new BusinessRuleException(duplicateMessage);
        }
    }

    private static DriverDto ToDto(Driver d) => new(d.DriverId, d.User.FullName, d.LicenceNo, d.Phone, d.IsAvailable);

    private static VehicleDto ToDto(Vehicle v) => new(v.VehicleId, v.Registration, v.Description, v.CapacityKg, v.IsActive);

    private static RouteDto ToDto(DeliveryRoute route)
    {
        var parcels = route.Parcels
            .OrderBy(rp => rp.StopSequence)
            .Select(rp => new RouteParcelDto(
                rp.ParcelId,
                rp.StopSequence,
                rp.Parcel.TrackingNo,
                rp.Parcel.RecipientName,
                rp.Parcel.DeliveryAddress,
                rp.Parcel.City,
                rp.Parcel.ServiceLevel,
                rp.Parcel.WeightKg,
                rp.Parcel.CurrentStatus))
            .ToList();

        return new RouteDto(
            route.RouteId,
            route.Name,
            route.RouteDate,
            route.DriverId,
            route.Driver.User.FullName,
            route.VehicleId,
            route.Vehicle.Registration,
            route.Vehicle.CapacityKg,
            parcels.Sum(p => p.WeightKg),
            route.Status,
            parcels);
    }
}
