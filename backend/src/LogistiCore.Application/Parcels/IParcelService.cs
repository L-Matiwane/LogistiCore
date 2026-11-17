namespace LogistiCore.Application.Parcels;

public interface IParcelService
{
    Task<PriceQuote> QuoteAsync(QuoteRequest request, CancellationToken ct = default);
    Task<ParcelDto> RegisterAsync(RegisterParcelRequest request, int userId, CancellationToken ct = default);
    Task<PagedResult<ParcelListItemDto>> SearchAsync(ParcelSearch search, CancellationToken ct = default);
    Task<ParcelDto?> GetAsync(int parcelId, CancellationToken ct = default);
}
