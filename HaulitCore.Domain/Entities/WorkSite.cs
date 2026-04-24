namespace HaulitCore.Domain.Entities;

#region Entity: Work Site

public class WorkSite
{
    #region Identity / Ownership

    public Guid Id { get; private set; } = Guid.NewGuid();

    // Tenant boundary (main account)
    public Guid OwnerUserId { get; private set; }

    #endregion

    #region Core Data

    public string Name { get; private set; } = string.Empty;

    // Optional company name (some sites may be internal yards)
    public string? CompanyName { get; private set; }

    public bool IsActive { get; private set; } = true;

    #endregion

    #region Optional Location (NEW)

    public string? AddressLine1 { get; private set; }   // NEW
    public string? AddressLine2 { get; private set; }   // NEW
    public string? Suburb { get; private set; }         // NEW
    public string? City { get; private set; }           // NEW
    public string? Region { get; private set; }         // NEW
    public string? Postcode { get; private set; }       // NEW
    public string? Country { get; private set; }        // NEW

    #endregion

    #region Constructors

    // Required by EF Core
    private WorkSite() { }

    public WorkSite(
        Guid ownerUserId,
        string name,
        string? companyName = null,
        string? addressLine1 = null,
        string? addressLine2 = null,
        string? suburb = null,
        string? city = null,
        string? region = null,
        string? postcode = null,
        string? country = null)
    {
        OwnerUserId = ownerUserId;
        Name = name.Trim();

        CompanyName = Clean(companyName);

        AddressLine1 = Clean(addressLine1);
        AddressLine2 = Clean(addressLine2);
        Suburb = Clean(suburb);
        City = Clean(city);
        Region = Clean(region);
        Postcode = Clean(postcode);
        Country = Clean(country);
    }

    #endregion

    #region Mutators

    public void Rename(string name) => Name = name.Trim();

    public void SetCompanyName(string? companyName) => CompanyName = Clean(companyName);

    public void UpdateAddress(
        string? addressLine1,
        string? addressLine2,
        string? suburb,
        string? city,
        string? region,
        string? postcode,
        string? country)
    {
        AddressLine1 = Clean(addressLine1);
        AddressLine2 = Clean(addressLine2);
        Suburb = Clean(suburb);
        City = Clean(city);
        Region = Clean(region);
        Postcode = Clean(postcode);
        Country = Clean(country);
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;

    #endregion

    #region Helpers

    private static string? Clean(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    #endregion
}

#endregion