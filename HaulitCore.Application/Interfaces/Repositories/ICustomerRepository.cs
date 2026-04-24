using HaulitCore.Domain.Entities;

namespace HaulitCore.Application.Interfaces.Repositories;

public interface ICustomerRepository
{
    Task AddAsync(Customer customer);
    Task UpdateAsync(Customer customer);

    Task<Customer?> GetByIdAsync(Guid id);
    Task<Customer?> GetByIdForUpdateAsync(Guid id);

    Task<IReadOnlyList<Customer>> GetByOwnerAsync(Guid ownerUserId);
    Task<IReadOnlyList<Customer>> SearchByOwnerAsync(Guid ownerUserId, string? searchTerm);

    Task<Customer?> FindMatchAsync(
        Guid ownerUserId,
        string companyName,
        string? email,
        string addressLine1,
        string city,
        string country);
}