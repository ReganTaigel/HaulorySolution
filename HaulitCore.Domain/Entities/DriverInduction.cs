using System;

namespace HaulitCore.Domain.Entities;

public class DriverInduction
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid OwnerUserId { get; private set; }
    public Guid DriverId { get; private set; }
    public Guid WorkSiteId { get; private set; }
    public Guid RequirementId { get; private set; }

    public ComplianceStatus Status { get; private set; } = ComplianceStatus.NotStarted;

    public DateTime IssueDateUtc { get; private set; }
    public DateTime? CompletedOnUtc { get; private set; }
    public DateTime? ExpiresOnUtc { get; private set; }
    public string? Notes { get; private set; }

    public string? EvidenceFileName { get; private set; }
    public string? EvidenceContentType { get; private set; }
    public string? EvidenceFilePath { get; private set; }
    public DateTime? EvidenceUploadedOnUtc { get; private set; }

    private DriverInduction() { }

    public DriverInduction(
        Guid ownerUserId,
        Guid driverId,
        Guid workSiteId,
        Guid requirementId,
        DateTime issueDateUtc)
    {
        OwnerUserId = ownerUserId;
        DriverId = driverId;
        WorkSiteId = workSiteId;
        RequirementId = requirementId;
        IssueDateUtc = DateTime.SpecifyKind(issueDateUtc, DateTimeKind.Utc);
    }

    public void SetStatus(ComplianceStatus status)
    {
        Status = status;
    }

    public void SetIssueDateUtc(DateTime issueDateUtc)
    {
        IssueDateUtc = DateTime.SpecifyKind(issueDateUtc, DateTimeKind.Utc);
    }

    public void SetCompletedOnUtc(DateTime? completedOnUtc)
    {
        CompletedOnUtc = completedOnUtc.HasValue
            ? DateTime.SpecifyKind(completedOnUtc.Value, DateTimeKind.Utc)
            : null;
    }

    public void SetExpiresOnUtc(DateTime? expiresOnUtc)
    {
        ExpiresOnUtc = expiresOnUtc.HasValue
            ? DateTime.SpecifyKind(expiresOnUtc.Value, DateTimeKind.Utc)
            : null;
    }

    public void SetNotes(string? notes)
    {
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    public void SetEvidence(string? fileName, string? contentType, string? filePath)
    {
        EvidenceFileName = string.IsNullOrWhiteSpace(fileName) ? null : fileName.Trim();
        EvidenceContentType = string.IsNullOrWhiteSpace(contentType) ? null : contentType.Trim();
        EvidenceFilePath = string.IsNullOrWhiteSpace(filePath) ? null : filePath.Trim();
        EvidenceUploadedOnUtc = EvidenceFilePath == null ? null : DateTime.UtcNow;
    }

    public void ClearEvidence()
    {
        EvidenceFileName = null;
        EvidenceContentType = null;
        EvidenceFilePath = null;
        EvidenceUploadedOnUtc = null;
    }
}