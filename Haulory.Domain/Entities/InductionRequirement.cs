namespace Haulory.Domain.Entities;

public class InductionRequirement
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid OwnerUserId { get; private set; }
    public Guid WorkSiteId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    // optional expiry rule
    public int? ValidForDays { get; private set; }

    // ✅ PPE field
    public string? PpeRequired { get; private set; }

    public bool IsActive { get; private set; } = true;

    public InductionRequirement() { } // EF

    public InductionRequirement(
        Guid ownerUserId,
        Guid workSiteId,
        string title,
        int? validForDays = null,
        string? ppeRequired = null)
    {
        OwnerUserId = ownerUserId;
        WorkSiteId = workSiteId;
        Title = title.Trim();
        ValidForDays = validForDays;
        PpeRequired = string.IsNullOrWhiteSpace(ppeRequired)
            ? null
            : ppeRequired.Trim();
    }

    public void Update(string title, int? validForDays, string? ppeRequired)
    {
        Title = title.Trim();
        ValidForDays = validForDays;
        PpeRequired = string.IsNullOrWhiteSpace(ppeRequired)
            ? null
            : ppeRequired.Trim();
    }
    public string ValidForDisplay =>
                    ValidForDays.HasValue && ValidForDays.Value > 0
                        ? $"Valid for: {ValidForDays.Value} days"
                        : "Valid for: Never expires";

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
