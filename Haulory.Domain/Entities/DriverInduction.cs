namespace Haulory.Domain.Entities;

public enum ComplianceStatus
{
    NotStarted = 0,
    InProgress = 1,
    Completed = 2,
    Expired = 3
}

public class DriverInduction
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid OwnerUserId { get; private set; }

    public Guid DriverId { get; private set; }
    public Guid WorkSiteId { get; private set; }
    public Guid RequirementId { get; private set; }

    public ComplianceStatus Status { get; private set; } = ComplianceStatus.NotStarted;

    public DateTime? CompletedOnUtc { get; private set; }
    public DateTime? ExpiresOnUtc { get; private set; }

    // ✅ anchor for "days left" comparisons
    public DateTime IssueDateUtc { get; private set; }

    public string? EvidenceFilePath { get; private set; }
    public string? Notes { get; private set; }

    // EF
    public DriverInduction() { }

    // ✅ existing constructor now sets issue date automatically
    public DriverInduction(Guid ownerUserId, Guid driverId, Guid workSiteId, Guid requirementId)
    {
        OwnerUserId = ownerUserId;
        DriverId = driverId;
        WorkSiteId = workSiteId;
        RequirementId = requirementId;

        IssueDateUtc = DateTime.UtcNow; // ✅ default
    }

    // ✅ new overload: create with an explicit issue date (what your UI wants)
    public DriverInduction(Guid ownerUserId, Guid driverId, Guid workSiteId, Guid requirementId, DateTime issueDateUtc)
        : this(ownerUserId, driverId, workSiteId, requirementId)
    {
        IssueDateUtc = DateTime.SpecifyKind(issueDateUtc, DateTimeKind.Utc);
    }

    public void MarkInProgress()
    {
        if (Status == ComplianceStatus.Completed) return;
        Status = ComplianceStatus.InProgress;
    }

    public void MarkCompleted(DateTime completedOnUtc, DateTime? expiresOnUtc, string? evidenceFilePath = null, string? notes = null)
    {
        CompletedOnUtc = completedOnUtc;
        ExpiresOnUtc = expiresOnUtc;
        EvidenceFilePath = string.IsNullOrWhiteSpace(evidenceFilePath) ? null : evidenceFilePath.Trim();
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        Status = ComplianceStatus.Completed;
    }

    public bool IsExpiringWithinDays(DateTime utcNow, int days)
    {
        if (!ExpiresOnUtc.HasValue) return false;
        var exp = ExpiresOnUtc.Value;
        return exp > utcNow && exp <= utcNow.AddDays(days);
    }

    public void MarkExpired() => Status = ComplianceStatus.Expired;

    public bool IsExpired(DateTime utcNow) =>
        ExpiresOnUtc.HasValue && ExpiresOnUtc.Value <= utcNow;

    public void SetStatus(ComplianceStatus status) => Status = status;
    public void SetCompletedOnUtc(DateTime? completedOnUtc) => CompletedOnUtc = completedOnUtc;
    public void SetExpiresOnUtc(DateTime? expiresOnUtc) => ExpiresOnUtc = expiresOnUtc;
    public void SetNotes(string? notes) => Notes = notes;

    // ✅ optional: allow changing the issue date later (if you want "edit relative dates")
    public void SetIssueDateUtc(DateTime issueDateUtc) =>
        IssueDateUtc = DateTime.SpecifyKind(issueDateUtc, DateTimeKind.Utc);
}
