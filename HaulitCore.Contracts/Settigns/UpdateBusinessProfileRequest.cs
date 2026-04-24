namespace HaulitCore.Contracts.Settings;

public sealed class UpdateBusinessProfileRequest
{
    public string? BusinessAddress1 { get; set; }
    public string? BusinessSuburb { get; set; }
    public string? BusinessCity { get; set; }
    public string? BusinessRegion { get; set; }
    public string? BusinessPostcode { get; set; }
    public string? BusinessCountry { get; set; }

    public string? SupplierGstNumber { get; set; }
    public string? SupplierNzbn { get; set; }
    public string? BankAccountNumber { get; set; }
}