namespace Haulory.Domain.Entities;

#region Entity: Driver Induction

public class DriverInduction
{
    #region Identity / Ownership

    public Guid Id { get; private set; } = Guid.NewGuid();

    // Tenant boundary
    public Guid OwnerUserId { get; private set; }

    public Guid DriverId { get; private set; }
    public Guid WorkSiteId { get; private set; }
    public Guid RequirementId { get; private set; }

    #endregion

    #region Status / Dates

    public ComplianceStatus Status { get; private set; } = ComplianceStatus.NotStarted;

    public DateTime? CompletedOnUtc { get; private set; }
    public DateTime? ExpiresOnUtc { get; private set; }

    // Anchor date for calculating expiry and days-left
    public DateTime IssueDateUtc { get; private set; }

    #endregion

    #region Evidence / Notes

    public string? EvidenceFilePath { get; private set; }
    public string? Notes { get; private set; }

    #endregion

    #region Constructors

    // Required by EF Core
    private DriverInduction() { }

    // Default constructor sets IssueDateUtc to now
    public DriverInduction(Guid ownerUserId, Guid driverId, Guid workSiteId, Guid requirementId)
    {
        OwnerUserId = ownerUserId;
        DriverId = driverId;
        WorkSiteId = workSiteId;
        RequirementId = requirementId;

        IssueDateUtc = DateTime.UtcNow;
    }

    // Overload allowing explicit issue date (used by UI workflows)
    public DriverInduction(
        Guid ownerUserId,
        Guid driverId,
        Guid workSiteId,
        Guid requirementId,
        DateTime issueDateUtc)
        : this(ownerUserId, driverId, workSiteId, requirementId)
    {
        IssueDateUtc = DateTime.SpecifyKind(issueDateUtc, DateTimeKind.Utc);
    }

    #endregion

    #region State Transitions

    public void MarkInProgress()
    {
        if (Status == ComplianceStatus.Completed)
            return;

        Status = ComplianceStatus.InProgress;
    }

    public void MarkCompleted(
        DateTime completedOnUtc,
        DateTime? expiresOnUtc,
        string? evidenceFilePath = null,
        string? notes = null)
    {
        CompletedOnUtc = DateTime.SpecifyKind(completedOnUtc, DateTimeKind.Utc);
        ExpiresOnUtc = expiresOnUtc.HasValue
            ? DateTime.SpecifyKind(expiresOnUtc.Value, DateTimeKind.Utc)
            : null;

        EvidenceFilePath = string.IsNullOrWhiteSpace(evidenceFilePath)
            ? null
            : evidenceFilePath.Trim();

        Notes = string.IsNullOrWhiteSpace(notes)
            ? null
            : notes.Trim();

        Status = ComplianceStatus.Completed;
    }

    public void MarkExpired()
    {
        Status = ComplianceStatus.Expired;
    }

    #endregion

    #region Expiry Logic

    public bool IsExpired(DateTime utcNow) =>
        ExpiresOnUtc.HasValue && ExpiresOnUtc.Value <= utcNow;

    public bool IsExpiringWithinDays(DateTime utcNow, int days)
    {
        if (!ExpiresOnUtc.HasValue)
            return false;

        var exp = ExpiresOnUtc.Value;

        return exp > utcNow && exp <= utcNow.AddDays(days);
    }

    #endregion

    #region Manual Setters (Used Carefully)

    public void SetStatus(ComplianceStatus status) => Status = status;

    public void SetCompletedOnUtc(DateTime? completedOnUtc) =>
        CompletedOnUtc = completedOnUtc;

    public void SetExpiresOnUtc(DateTime? expiresOnUtc) =>
        ExpiresOnUtc = expiresOnUtc;

    public void SetNotes(string? notes) =>
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();

    public void SetIssueDateUtc(DateTime issueDateUtc) =>
        IssueDateUtc = DateTime.SpecifyKind(issueDateUtc, DateTimeKind.Utc);

    #endregion
}

#endregion