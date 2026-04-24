using HaulitCore.Application.Interfaces.Repositories;
using HaulitCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HaulitCore.Infrastructure.Persistence.Repositories;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly HaulitCoreDbContext _db;

    public CustomerRepository(HaulitCoreDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Customer customer)
    {
        if (customer == null) throw new ArgumentNullException(nameof(customer));

        var tracked = _db.Set<Customer>().Local.FirstOrDefault(x => x.Id == customer.Id);
        if (tracked != null)
            return;

        var exists = await _db.Set<Customer>().AnyAsync(x => x.Id == customer.Id);
        if (exists)
            return;

        _db.Set<Customer>().Add(customer);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Customer customer)
    {
        if (customer == null) throw new ArgumentNullException(nameof(customer));

        var target = _db.Set<Customer>().Local.FirstOrDefault(x => x.Id == customer.Id)
                     ?? await _db.Set<Customer>().FirstOrDefaultAsync(x => x.Id == customer.Id);

        if (target == null)
            throw new KeyNotFoundException($"Customer not found: {customer.Id}");

        await _db.SaveChangesAsync();
    }

    public async Task<Customer?> GetByIdAsync(Guid id)
        => await _db.Set<Customer>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<Customer?> GetByIdForUpdateAsync(Guid id)
        => await _db.Set<Customer>()
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<IReadOnlyList<Customer>> GetByOwnerAsync(Guid ownerUserId)
    {
        if (ownerUserId == Guid.Empty)
            return Array.Empty<Customer>();

        return await _db.Set<Customer>()
            .AsNoTracking()
            .Where(x => x.OwnerUserId == ownerUserId)
            .OrderBy(x => x.CompanyName)
            .ThenBy(x => x.ContactName)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Customer>> SearchByOwnerAsync(Guid ownerUserId, string? searchTerm)
    {
        if (ownerUserId == Guid.Empty)
            return Array.Empty<Customer>();

        var query = _db.Set<Customer>()
            .AsNoTracking()
            .Where(x => x.OwnerUserId == ownerUserId);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var token = searchTerm.Trim().ToLower();

            query = query.Where(x =>
                x.CompanyName.ToLower().Contains(token) ||
                (x.ContactName != null && x.ContactName.ToLower().Contains(token)) ||
                (x.Email != null && x.Email.ToLower().Contains(token)) ||
                x.AddressLine1.ToLower().Contains(token) ||
                x.City.ToLower().Contains(token));
        }

        return await query
            .OrderBy(x => x.CompanyName)
            .ThenBy(x => x.ContactName)
            .ToListAsync();
    }

    public async Task<Customer?> FindMatchAsync(
        Guid ownerUserId,
        string companyName,
        string? email,
        string addressLine1,
        string city,
        string country)
    {
        if (ownerUserId == Guid.Empty)
            return null;

        var normalizedCompany = (companyName ?? string.Empty).Trim().ToLower();
        var normalizedEmail = string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLower();
        var normalizedAddress = (addressLine1 ?? string.Empty).Trim().ToLower();
        var normalizedCity = (city ?? string.Empty).Trim().ToLower();
        var normalizedCountry = (country ?? string.Empty).Trim().ToLower();

        return await _db.Set<Customer>()
            .FirstOrDefaultAsync(x =>
                x.OwnerUserId == ownerUserId &&
                x.CompanyName.ToLower() == normalizedCompany &&
                (normalizedEmail == null || (x.Email != null && x.Email.ToLower() == normalizedEmail)) &&
                x.AddressLine1.ToLower() == normalizedAddress &&
                x.City.ToLower() == normalizedCity &&
                x.Country.ToLower() == normalizedCountry);
    }
}