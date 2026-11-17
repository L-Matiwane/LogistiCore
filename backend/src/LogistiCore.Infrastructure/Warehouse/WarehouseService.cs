using LogistiCore.Application.Warehouse;
using LogistiCore.Domain.Entities;
using LogistiCore.Domain.Rules;
using LogistiCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LogistiCore.Infrastructure.Warehouse;

public class WarehouseService : IWarehouseService
{
    private readonly AppDbContext _db;

    public WarehouseService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<BinDto>> ListBinsAsync(CancellationToken ct = default)
    {
        var groups = await _db.Parcels
            .AsNoTracking()
            .Where(p => p.BinId != null &&
                        (p.CurrentStatus == ParcelStatus.AtWarehouse || p.CurrentStatus == ParcelStatus.Sorted))
            .GroupBy(p => p.BinId)
            .Select(g => new { BinId = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var counts = groups
            .Where(g => g.BinId.HasValue)
            .ToDictionary(g => g.BinId!.Value, g => g.Count);

        var bins = await _db.Bins.AsNoTracking().OrderBy(b => b.Zone).ThenBy(b => b.Code).ToListAsync(ct);

        return bins
            .Select(b => new BinDto(b.BinId, b.Zone, b.Code, b.Capacity, counts.GetValueOrDefault(b.BinId)))
            .ToList();
    }

    public async Task<IReadOnlyList<InventoryItemDto>> ListInventoryAsync(int? binId, CancellationToken ct = default)
    {
        var query = _db.Parcels
            .AsNoTracking()
            .Where(p => p.CurrentStatus == ParcelStatus.AtWarehouse || p.CurrentStatus == ParcelStatus.Sorted);

        if (binId is int id)
        {
            query = query.Where(p => p.BinId == id);
        }

        return await query
            .OrderByDescending(p => p.RegisteredAt)
            .Take(200)
            .Select(p => new InventoryItemDto(
                p.ParcelId,
                p.TrackingNo,
                p.RecipientName,
                p.City,
                p.ServiceLevel,
                p.CurrentStatus,
                p.Bin == null ? null : p.Bin.Code))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ExceptionDto>> ListExceptionsAsync(CancellationToken ct = default)
    {
        var query =
            from e in _db.ParcelExceptions.AsNoTracking()
            join p in _db.Parcels on e.ParcelId equals p.ParcelId
            join u in _db.Users on e.ReportedByUserId equals u.UserId into reporters
            from reporter in reporters.DefaultIfEmpty()
            orderby e.ReportedAt descending
            select new ExceptionDto(
                e.ExceptionId,
                p.TrackingNo,
                p.Customer.Name,
                e.Type,
                e.Reason,
                reporter == null ? null : reporter.FullName,
                e.ReportedAt,
                e.Resolved);

        return await query.Take(50).ToListAsync(ct);
    }

    public async Task<ScanResultDto> ReceiveAsync(ScanRequest request, int userId, CancellationToken ct = default)
    {
        var parcel = await FindParcelAsync(request.TrackingNo, ct);

        if (!ParcelStatusRules.CanTransition(parcel.CurrentStatus, ParcelStatus.AtWarehouse))
        {
            throw new BusinessRuleException(
                $"{parcel.TrackingNo} cannot be received because its status is {ParcelStatusRules.Label(parcel.CurrentStatus)}.");
        }

        parcel.CurrentStatus = ParcelStatus.AtWarehouse;
        parcel.BinId = null;
        AddHistory(parcel, ParcelStatus.AtWarehouse, userId, "Received at warehouse");
        await _db.SaveChangesAsync(ct);

        return ToResult(parcel, null, $"Received {parcel.TrackingNo}. Next, sort it to a bin.");
    }

    public async Task<ScanResultDto> SortAsync(SortRequest request, int userId, CancellationToken ct = default)
    {
        var parcel = await FindParcelAsync(request.TrackingNo, ct);

        if (!ParcelStatusRules.IsInWarehouse(parcel.CurrentStatus))
        {
            throw new BusinessRuleException(
                $"{parcel.TrackingNo} must be received at the warehouse first. Its status is {ParcelStatusRules.Label(parcel.CurrentStatus)}.");
        }

        var bin = await _db.Bins.FirstOrDefaultAsync(b => b.BinId == request.BinId, ct)
            ?? throw new BusinessRuleException("That bin does not exist.");

        if (parcel.BinId == bin.BinId)
        {
            throw new BusinessRuleException($"{parcel.TrackingNo} is already in bin {bin.Code}.");
        }

        var occupied = await _db.Parcels.CountAsync(
            p => p.BinId == bin.BinId &&
                 p.ParcelId != parcel.ParcelId &&
                 (p.CurrentStatus == ParcelStatus.AtWarehouse || p.CurrentStatus == ParcelStatus.Sorted),
            ct);

        if (occupied >= bin.Capacity)
        {
            throw new BusinessRuleException($"Bin {bin.Code} is full ({occupied} of {bin.Capacity}). Choose another bin.");
        }

        var moved = parcel.CurrentStatus == ParcelStatus.Sorted;
        parcel.BinId = bin.BinId;
        parcel.CurrentStatus = ParcelStatus.Sorted;
        AddHistory(parcel, ParcelStatus.Sorted, userId, moved ? $"Moved to bin {bin.Code}" : $"Sorted to bin {bin.Code} ({bin.Zone})");
        await _db.SaveChangesAsync(ct);

        return ToResult(parcel, bin.Code, $"{parcel.TrackingNo} is now in bin {bin.Code}.");
    }

    public async Task<ScanResultDto> ReportExceptionAsync(ExceptionRequest request, int userId, CancellationToken ct = default)
    {
        var parcel = await FindParcelAsync(request.TrackingNo, ct);

        if (!ParcelStatusRules.CanTransition(parcel.CurrentStatus, request.Type))
        {
            throw new BusinessRuleException(
                $"{parcel.TrackingNo} cannot be flagged as {request.Type.ToLowerInvariant()} because its status is {ParcelStatusRules.Label(parcel.CurrentStatus)}.");
        }

        var reason = request.Reason.Trim();
        parcel.CurrentStatus = request.Type;
        parcel.BinId = null;

        _db.ParcelExceptions.Add(new ParcelException
        {
            ParcelId = parcel.ParcelId,
            Type = request.Type,
            Reason = reason,
            ReportedByUserId = userId,
            ReportedAt = DateTime.UtcNow,
            Resolved = false
        });
        AddHistory(parcel, request.Type, userId, reason);
        await _db.SaveChangesAsync(ct);

        return ToResult(parcel, null, $"{parcel.TrackingNo} flagged as {request.Type.ToLowerInvariant()}.");
    }

    private async Task<Parcel> FindParcelAsync(string trackingNo, CancellationToken ct)
    {
        var code = trackingNo.Trim().ToUpperInvariant();
        return await _db.Parcels.FirstOrDefaultAsync(p => p.TrackingNo == code, ct)
            ?? throw new BusinessRuleException($"No parcel found with tracking number {code}.");
    }

    private void AddHistory(Parcel parcel, string status, int userId, string note) =>
        _db.StatusHistory.Add(new ParcelStatusHistory
        {
            ParcelId = parcel.ParcelId,
            Status = status,
            ChangedByUserId = userId,
            ChangedAt = DateTime.UtcNow,
            Note = note
        });

    private static ScanResultDto ToResult(Parcel parcel, string? binCode, string message) =>
        new(parcel.ParcelId, parcel.TrackingNo, parcel.RecipientName, parcel.City, parcel.ServiceLevel, parcel.CurrentStatus, binCode, message);
}
