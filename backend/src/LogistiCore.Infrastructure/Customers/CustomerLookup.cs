using LogistiCore.Application.Customers;
using LogistiCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LogistiCore.Infrastructure.Customers;

public class CustomerLookup : ICustomerLookup
{
    private readonly AppDbContext _db;

    public CustomerLookup(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<CustomerOptionDto>> ListActiveAsync(CancellationToken ct = default) =>
        await _db.Customers
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new CustomerOptionDto(c.CustomerId, c.AccountNo, c.Name))
            .ToListAsync(ct);
}
