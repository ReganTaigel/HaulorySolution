public class WorkSite
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OwnerUserId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? CompanyName { get; private set; }  // optional

    public bool IsActive { get; private set; } = true;

    public WorkSite() { }

    public WorkSite(Guid ownerUserId, string name, string? companyName = null)
    {
        OwnerUserId = ownerUserId;
        Name = name.Trim();
        CompanyName = string.IsNullOrWhiteSpace(companyName) ? null : companyName.Trim();
    }

    public void Rename(string name) => Name = name.Trim();
    public void SetCompanyName(string? companyName) =>
        CompanyName = string.IsNullOrWhiteSpace(companyName) ? null : companyName.Trim();
}
