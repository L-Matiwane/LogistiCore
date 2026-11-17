namespace LogistiCore.Application.Customers;

public record CustomerOptionDto(int CustomerId, string AccountNo, string Name);

public interface ICustomerLookup
{
    Task<IReadOnlyList<CustomerOptionDto>> ListActiveAsync(CancellationToken ct = default);
}
