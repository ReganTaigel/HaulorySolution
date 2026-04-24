using HaulitCore.Application.Interfaces.Services;
using HaulitCore.Contracts.Customers;

namespace HaulitCore.Mobile.Services;

public sealed class CustomersApiService : ApiServiceBase
{
    public CustomersApiService(HttpClient httpClient, ISessionService sessionService)
        : base(httpClient, sessionService)
    {
    }

    public async Task<IReadOnlyList<CustomerDto>> GetCustomersAsync(string? search = null, CancellationToken cancellationToken = default)
    {
        var url = string.IsNullOrWhiteSpace(search)
            ? "api/customers"
            : $"api/customers?search={Uri.EscapeDataString(search)}";

        return await GetAsync<List<CustomerDto>>(url, cancellationToken) ?? new List<CustomerDto>();
    }

    public Task<CustomerDto?> GetCustomerByIdAsync(Guid customerId, CancellationToken cancellationToken = default)
        => GetAsync<CustomerDto>($"api/customers/{customerId}", cancellationToken);
}