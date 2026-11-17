namespace LogistiCore.Application.Parcels;

public record QuoteRequest(decimal WeightKg, decimal? LengthCm, decimal? WidthCm, decimal? HeightCm, string ServiceLevel);

public record PriceQuote(decimal ActualWeightKg, decimal VolumetricWeightKg, decimal BilledWeightKg, string ServiceLevel, decimal Price);

public record RegisterParcelRequest(
    int CustomerId,
    string RecipientName,
    string RecipientPhone,
    string? RecipientEmail,
    string DeliveryAddress,
    string City,
    string PostalCode,
    string? DeliveryInstructions,
    decimal WeightKg,
    decimal? LengthCm,
    decimal? WidthCm,
    decimal? HeightCm,
    string ServiceLevel,
    string[]? SpecialHandling);

public record StatusHistoryDto(string Status, string? ChangedByName, DateTime ChangedAt, string? Note);

public record ParcelDto(
    int ParcelId,
    string TrackingNo,
    int CustomerId,
    string CustomerName,
    string RecipientName,
    string RecipientPhone,
    string? RecipientEmail,
    string DeliveryAddress,
    string City,
    string PostalCode,
    string? DeliveryInstructions,
    string[] SpecialHandling,
    decimal WeightKg,
    decimal? LengthCm,
    decimal? WidthCm,
    decimal? HeightCm,
    decimal BilledWeightKg,
    string ServiceLevel,
    decimal Price,
    string CurrentStatus,
    DateTime RegisteredAt,
    IReadOnlyList<StatusHistoryDto> History);

public record ParcelListItemDto(
    int ParcelId,
    string TrackingNo,
    string CustomerName,
    string RecipientName,
    string DeliveryAddress,
    string City,
    string ServiceLevel,
    decimal Price,
    string CurrentStatus,
    DateTime RegisteredAt);

public record ParcelSearch(string? Q, string? Status, int? CustomerId, DateOnly? From, DateOnly? To, int Page, int PageSize);

public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
