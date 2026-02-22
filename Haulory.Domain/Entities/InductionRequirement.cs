namespace Haulory.Domain.Entities;

#region Entity: Induction Requirement

public class InductionRequirement
{
    #region Identity / Ownership

    public Guid Id { get; private set; } = Guid.NewGuid();

    // Tenant boundary
    public Guid OwnerUserId { get; private set; }

    // Worksite this requirement belongs to
    public Guid WorkSiteId { get; private set; }

    #endregion

    #region Core Data

    public string Title { get; private set; } = string.Empty;

    // Optional expiry rule (null = never expires)
    public int? ValidForDays { get; private set; }

    // Optional PPE requirements (comma-separated or descriptive)
    public string? PpeRequired { get; private set; }

    public bool IsActive { get; private set; } = true;

    #endregion

    #region Constructors

    private InductionRequirement() { }

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

    #endregion

    #region Mutators

    public void Update(string title, int? validForDays, string? ppeRequired)
    {
        Title = title.Trim();
        ValidForDays = validForDays;

        PpeRequired = string.IsNullOrWhiteSpace(ppeRequired)
            ? null
            : ppeRequired.Trim();
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;

    #endregion

    #region Derived Properties

    public string ValidForDisplay =>
        ValidForDays.HasValue && ValidForDays.Value > 0
            ? $"Valid for: {ValidForDays.Value} days"
            : "Valid for: Never expires";

    #endregion
}

#endregion