using LogistiCore.Domain.Rules;

namespace LogistiCore.Application.Warehouse;

public record ScanRequest(string TrackingNo);
public record SortRequest(string TrackingNo, int BinId);
public record ExceptionRequest(string TrackingNo, string Type, string Reason);

public record BinDto(int BinId, string Zone, string Code, int Capacity, int Occupied);

public record ScanResultDto(
    int ParcelId,
    string TrackingNo,
    string RecipientName,
    string City,
    string ServiceLevel,
    string CurrentStatus,
    string? BinCode,
    string Message);

public record InventoryItemDto(
    int ParcelId,
    string TrackingNo,
    string RecipientName,
    string City,
    string ServiceLevel,
    string CurrentStatus,
    string? BinCode);

public record ExceptionDto(
    int ExceptionId,
    string TrackingNo,
    string CustomerName,
    string Type,
    string Reason,
    string? ReportedByName,
    DateTime ReportedAt,
    bool Resolved);

public interface IWarehouseService
{
    Task<IReadOnlyList<BinDto>> ListBinsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<InventoryItemDto>> ListInventoryAsync(int? binId, CancellationToken ct = default);
    Task<IReadOnlyList<ExceptionDto>> ListExceptionsAsync(CancellationToken ct = default);
    Task<ScanResultDto> ReceiveAsync(ScanRequest request, int userId, CancellationToken ct = default);
    Task<ScanResultDto> SortAsync(SortRequest request, int userId, CancellationToken ct = default);
    Task<ScanResultDto> ReportExceptionAsync(ExceptionRequest request, int userId, CancellationToken ct = default);
}

public static class WarehouseValidation
{
    public static Dictionary<string, string[]> ValidateScan(ScanRequest r)
    {
        var errors = new Dictionary<string, string[]>();
        CheckTracking(errors, r.TrackingNo);
        return errors;
    }

    public static Dictionary<string, string[]> ValidateSort(SortRequest r)
    {
        var errors = new Dictionary<string, string[]>();
        CheckTracking(errors, r.TrackingNo);
        if (r.BinId <= 0)
        {
            errors["binId"] = new[] { "Choose a bin." };
        }
        return errors;
    }

    public static Dictionary<string, string[]> ValidateException(ExceptionRequest r)
    {
        var errors = new Dictionary<string, string[]>();
        CheckTracking(errors, r.TrackingNo);

        if (r.Type != ParcelStatus.Lost && r.Type != ParcelStatus.Damaged)
        {
            errors["type"] = new[] { "Choose Lost or Damaged." };
        }

        if (string.IsNullOrWhiteSpace(r.Reason))
        {
            errors["reason"] = new[] { "Describe what happened." };
        }
        else if (r.Reason.Trim().Length > 500)
        {
            errors["reason"] = new[] { "The reason must be 500 characters or fewer." };
        }
        return errors;
    }

    private static void CheckTracking(Dictionary<string, string[]> errors, string? trackingNo)
    {
        if (string.IsNullOrWhiteSpace(trackingNo))
        {
            errors["trackingNo"] = new[] { "Scan or type a tracking number." };
        }
        else if (trackingNo.Trim().Length > 30)
        {
            errors["trackingNo"] = new[] { "That tracking number is too long." };
        }
    }
}
