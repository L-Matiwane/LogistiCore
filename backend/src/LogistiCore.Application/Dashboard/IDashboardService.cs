namespace LogistiCore.Application.Dashboard;

public record DailyVolumeDto(string Date, int Count);

public record DashboardSummaryDto(
    int ParcelsToday,
    int InWarehouse,
    int OutForDelivery,
    int DeliveredToday,
    int Exceptions,
    IReadOnlyList<DailyVolumeDto> WeeklyVolume);

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken ct = default);
}
