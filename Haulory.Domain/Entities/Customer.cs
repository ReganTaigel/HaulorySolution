namespace Haulory.Domain.Entities;

public class Customer
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OwnerUserId { get; private set; }

    public string CompanyName { get; private set; } = string.Empty;
    public string? ContactName { get; private set; }
    public string? Email { get; private set; }
    public string AddressLine1 { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string Country { get; private set; } = "New Zealand";

    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; private set; }

    private Customer() { }

    public Customer(
        Guid id,
        Guid ownerUserId,
        string companyName,
        string? contactName,
        string? email,
        string addressLine1,
        string city,
        string country)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Customer id is required.", nameof(id));

        if (ownerUserId == Guid.Empty)
            throw new ArgumentException("Owner user id is required.", nameof(ownerUserId));

        Id = id;
        OwnerUserId = ownerUserId;

        UpdateDetails(companyName, contactName, email, addressLine1, city, country);
    }

    public void UpdateDetails(
        string companyName,
        string? contactName,
        string? email,
        string addressLine1,
        string city,
        string country)
    {
        CompanyName = companyName?.Trim() ?? string.Empty;
        ContactName = string.IsNullOrWhiteSpace(contactName) ? null : contactName.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        AddressLine1 = addressLine1?.Trim() ?? string.Empty;
        City = city?.Trim() ?? string.Empty;
        Country = string.IsNullOrWhiteSpace(country) ? "New Zealand" : country.Trim();
        UpdatedAtUtc = DateTime.UtcNow;
    }
}