using LogistiCore.Application.Dashboard;
using LogistiCore.Domain.Rules;
using LogistiCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LogistiCore.Infrastructure.Dashboard;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;

    public DashboardService(AppDbContext db) => _db = db;

    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken ct = default)
    {
        var nowUtc = DateTime.UtcNow;
        var todayStartUtc = LocalTime.StartOfLocalDayUtc(nowUtc);

        var parcelsToday = await _db.Parcels.CountAsync(p => p.RegisteredAt >= todayStartUtc, ct);

        var inWarehouse = await _db.Parcels.CountAsync(
            p => p.CurrentStatus == ParcelStatus.AtWarehouse || p.CurrentStatus == ParcelStatus.Sorted, ct);

        var outForDelivery = await _db.Parcels.CountAsync(p => p.CurrentStatus == ParcelStatus.OutForDelivery, ct);

        var deliveredToday = await _db.StatusHistory
            .Where(h => h.Status == ParcelStatus.Delivered && h.ChangedAt >= todayStartUtc)
            .Select(h => h.ParcelId)
            .Distinct()
            .CountAsync(ct);

        var exceptions = await _db.Parcels.CountAsync(
            p => p.CurrentStatus == ParcelStatus.Lost ||
                 p.CurrentStatus == ParcelStatus.Damaged ||
                 p.CurrentStatus == ParcelStatus.FailedAttempt, ct);

        var weekStartUtc = todayStartUtc.AddDays(-6);
        var registered = await _db.Parcels
            .AsNoTracking()
            .Where(p => p.RegisteredAt >= weekStartUtc)
            .Select(p => p.RegisteredAt)
            .ToListAsync(ct);

        var perDay = registered
            .GroupBy(d => LocalTime.ToLocal(d).Date)
            .ToDictionary(g => g.Key, g => g.Count());

        var firstLocalDay = LocalTime.ToLocal(weekStartUtc).Date;
        var weekly = Enumerable.Range(0, 7)
            .Select(i =>
            {
                var day = firstLocalDay.AddDays(i);
                return new DailyVolumeDto(day.ToString("yyyy-MM-dd"), perDay.GetValueOrDefault(day));
            })
            .ToList();

        return new DashboardSummaryDto(parcelsToday, inWarehouse, outForDelivery, deliveredToday, exceptions, weekly);
    }
}
