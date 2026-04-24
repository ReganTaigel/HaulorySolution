namespace HaulitCore.Contracts.Customers;

public sealed class CustomerDto
{
    public Guid Id { get; set; }
    public Guid OwnerUserId { get; set; }

    public string CompanyName { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? Email { get; set; }
    public string AddressLine1 { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}