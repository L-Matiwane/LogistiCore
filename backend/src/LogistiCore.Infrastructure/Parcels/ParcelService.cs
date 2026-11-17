using LogistiCore.Application.Parcels;
using LogistiCore.Domain.Entities;
using LogistiCore.Domain.Rules;
using LogistiCore.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LogistiCore.Infrastructure.Parcels;

public class ParcelService : IParcelService
{
    private const int MaxTrackingAttempts = 5;
    private readonly AppDbContext _db;

    public ParcelService(AppDbContext db) => _db = db;

    public async Task<PriceQuote> QuoteAsync(QuoteRequest request, CancellationToken ct = default)
    {
        var weight = Math.Round(request.WeightKg, 2, MidpointRounding.AwayFromZero);
        var volumetric = PricingRules.VolumetricWeightKg(request.LengthCm, request.WidthCm, request.HeightCm);
        var billed = PricingRules.BilledWeightKg(weight, volumetric);
        var price = await FindPriceAsync(request.ServiceLevel, billed, ct);

        return new PriceQuote(weight, volumetric, billed, request.ServiceLevel, price);
    }

    public async Task<ParcelDto> RegisterAsync(RegisterParcelRequest request, int userId, CancellationToken ct = default)
    {
        var customerExists = await _db.Customers
            .AsNoTracking()
            .AnyAsync(c => c.CustomerId == request.CustomerId && c.IsActive, ct);

        if (!customerExists)
        {
            throw new BusinessRuleException("The selected customer account does not exist or is inactive.");
        }

        var weight = Math.Round(request.WeightKg, 2, MidpointRounding.AwayFromZero);
        var volumetric = PricingRules.VolumetricWeightKg(request.LengthCm, request.WidthCm, request.HeightCm);
        var billed = PricingRules.BilledWeightKg(weight, volumetric);
        var price = await FindPriceAsync(request.ServiceLevel, billed, ct);

        var handling = (request.SpecialHandling ?? Array.Empty<string>())
            .Where(tag => HandlingTags.All.Contains(tag))
            .Distinct()
            .ToArray();

        for (var attempt = 1; ; attempt++)
        {
            var nowUtc = DateTime.UtcNow;
            var trackingNo = await NextTrackingNumberAsync(nowUtc, ct);

            var parcel = new Parcel
            {
                TrackingNo = trackingNo,
                CustomerId = request.CustomerId,
                RecipientName = request.RecipientName.Trim(),
                RecipientPhone = request.RecipientPhone.Trim(),
                RecipientEmail = string.IsNullOrWhiteSpace(request.RecipientEmail) ? null : request.RecipientEmail.Trim(),
                DeliveryAddress = request.DeliveryAddress.Trim(),
                City = request.City.Trim(),
                PostalCode = request.PostalCode.Trim(),
                DeliveryInstructions = string.IsNullOrWhiteSpace(request.DeliveryInstructions) ? null : request.DeliveryInstructions.Trim(),
                SpecialHandling = handling.Length == 0 ? null : string.Join(',', handling),
                WeightKg = weight,
                LengthCm = request.LengthCm,
                WidthCm = request.WidthCm,
                HeightCm = request.HeightCm,
                BilledWeightKg = billed,
                ServiceLevel = request.ServiceLevel,
                Price = price,
                CurrentStatus = ParcelStatus.Registered,
                RegisteredByUserId = userId,
                RegisteredAt = nowUtc
            };

            parcel.History.Add(new ParcelStatusHistory
            {
                Status = ParcelStatus.Registered,
                ChangedByUserId = userId,
                ChangedAt = nowUtc,
                Note = "Parcel registered"
            });

            _db.Parcels.Add(parcel);

            try
            {
                await _db.SaveChangesAsync(ct);
                return (await GetAsync(parcel.ParcelId, ct))!;
            }
            catch (DbUpdateException ex) when (IsDuplicateKey(ex) && attempt < MaxTrackingAttempts)
            {
                // Another clerk took this tracking number a moment ago. Forget this attempt and try the next number.
                _db.ChangeTracker.Clear();
            }
        }
    }

    public async Task<PagedResult<ParcelListItemDto>> SearchAsync(ParcelSearch search, CancellationToken ct = default)
    {
        var page = Math.Max(1, search.Page);
        var pageSize = Math.Clamp(search.PageSize, 1, 100);

        var query = _db.Parcels.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search.Q))
        {
            var q = search.Q.Trim();
            query = query.Where(p =>
                p.TrackingNo.Contains(q) ||
                p.RecipientName.Contains(q) ||
                p.Customer.Name.Contains(q));
        }

        if (!string.IsNullOrWhiteSpace(search.Status))
        {
            var status = search.Status;
            query = query.Where(p => p.CurrentStatus == status);
        }

        if (search.CustomerId is int customerId)
        {
            query = query.Where(p => p.CustomerId == customerId);
        }

        if (search.From is DateOnly from)
        {
            var fromUtc = from.ToDateTime(TimeOnly.MinValue) - LocalTime.Offset;
            query = query.Where(p => p.RegisteredAt >= fromUtc);
        }

        if (search.To is DateOnly to)
        {
            var toUtc = to.AddDays(1).ToDateTime(TimeOnly.MinValue) - LocalTime.Offset;
            query = query.Where(p => p.RegisteredAt < toUtc);
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(p => p.RegisteredAt)
            .ThenByDescending(p => p.ParcelId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ParcelListItemDto(
                p.ParcelId,
                p.TrackingNo,
                p.Customer.Name,
                p.RecipientName,
                p.DeliveryAddress,
                p.City,
                p.ServiceLevel,
                p.Price,
                p.CurrentStatus,
                p.RegisteredAt))
            .ToListAsync(ct);

        return new PagedResult<ParcelListItemDto>(items, page, pageSize, total);
    }

    public async Task<ParcelDto?> GetAsync(int parcelId, CancellationToken ct = default)
    {
        var parcel = await _db.Parcels
            .AsNoTracking()
            .Include(p => p.Customer)
            .Include(p => p.History)
            .FirstOrDefaultAsync(p => p.ParcelId == parcelId, ct);

        if (parcel is null)
        {
            return null;
        }

        var userIds = parcel.History
            .Where(h => h.ChangedByUserId != null)
            .Select(h => h.ChangedByUserId!.Value)
            .Distinct()
            .ToList();

        var names = await _db.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.UserId))
            .ToDictionaryAsync(u => u.UserId, u => u.FullName, ct);

        var history = parcel.History
            .OrderBy(h => h.ChangedAt)
            .ThenBy(h => h.HistoryId)
            .Select(h => new StatusHistoryDto(
                h.Status,
                h.ChangedByUserId is int id && names.TryGetValue(id, out var name) ? name : null,
                h.ChangedAt,
                h.Note))
            .ToList();

        var handling = string.IsNullOrWhiteSpace(parcel.SpecialHandling)
            ? Array.Empty<string>()
            : parcel.SpecialHandling.Split(',', StringSplitOptions.RemoveEmptyEntries);

        return new ParcelDto(
            parcel.ParcelId,
            parcel.TrackingNo,
            parcel.CustomerId,
            parcel.Customer.Name,
            parcel.RecipientName,
            parcel.RecipientPhone,
            parcel.RecipientEmail,
            parcel.DeliveryAddress,
            parcel.City,
            parcel.PostalCode,
            parcel.DeliveryInstructions,
            handling,
            parcel.WeightKg,
            parcel.LengthCm,
            parcel.WidthCm,
            parcel.HeightCm,
            parcel.BilledWeightKg,
            parcel.ServiceLevel,
            parcel.Price,
            parcel.CurrentStatus,
            parcel.RegisteredAt,
            history);
    }

    private async Task<decimal> FindPriceAsync(string serviceLevel, decimal billedWeightKg, CancellationToken ct)
    {
        // A band covers weights above its minimum up to and including its maximum.
        var rate = await _db.RateCards
            .AsNoTracking()
            .Where(r => r.ServiceLevel == serviceLevel && r.MinWeightKg < billedWeightKg && billedWeightKg <= r.MaxWeightKg)
            .OrderBy(r => r.MinWeightKg)
            .FirstOrDefaultAsync(ct);

        if (rate is null)
        {
            throw new BusinessRuleException(
                $"No rate is configured for {serviceLevel} at a billed weight of {billedWeightKg:0.##} kg.");
        }

        return rate.Price;
    }

    private async Task<string> NextTrackingNumberAsync(DateTime nowUtc, CancellationToken ct)
    {
        var local = LocalTime.ToLocal(nowUtc);
        var prefix = TrackingNumber.Prefix(local);

        var last = await _db.Parcels
            .AsNoTracking()
            .Where(p => p.TrackingNo.StartsWith(prefix))
            .OrderByDescending(p => p.TrackingNo)
            .Select(p => p.TrackingNo)
            .FirstOrDefaultAsync(ct);

        var next = last is not null && TrackingNumber.TryGetSequence(last, out var sequence) ? sequence + 1 : 1;
        return TrackingNumber.Format(local, next);
    }

    private static bool IsDuplicateKey(DbUpdateException ex) =>
        ex.InnerException is SqlException { Number: 2601 or 2627 };
}
