namespace Haulory.Domain.Entities;

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

    #region Constructors

    // Required by EF Core
    private WorkSite() { }

    public WorkSite(Guid ownerUserId, string name, string? companyName = null)
    {
        OwnerUserId = ownerUserId;
        Name = name.Trim();

        CompanyName = string.IsNullOrWhiteSpace(companyName)
            ? null
            : companyName.Trim();
    }

    #endregion

    #region Mutators

    public void Rename(string name)
    {
        Name = name.Trim();
    }

    public void SetCompanyName(string? companyName)
    {
        CompanyName = string.IsNullOrWhiteSpace(companyName)
            ? null
            : companyName.Trim();
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;

    #endregion
}

#endregion
