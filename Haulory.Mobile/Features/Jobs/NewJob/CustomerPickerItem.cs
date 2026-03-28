namespace Haulory.Mobile.Features.Jobs.NewJob;

public sealed class CustomerPickerItem
{
    public Guid Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? Email { get; set; }
    public string AddressLine1 { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

    public string DisplayName =>
        string.IsNullOrWhiteSpace(ContactName)
            ? CompanyName
            : $"{CompanyName} ({ContactName})";
}