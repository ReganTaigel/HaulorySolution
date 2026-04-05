using System;
using Haulory.Domain.Entities;

namespace Haulory.Application.Features.Incductions;

// Represents a flattened view of a driver's induction/compliance requirement.
// Designed for list views and UI consumption.
public class DriverInductionListItemDto
{
    // Unique identifier for the driver induction record.
    public Guid DriverInductionId { get; set; }

    // Work site details associated with the induction.
    public Guid WorkSiteId { get; set; }
    public string WorkSiteName { get; set; } = string.Empty;

    // Requirement details (e.g., safety requirement or certification).
    public Guid RequirementId { get; set; }
    public string RequirementTitle { get; set; } = string.Empty;

    // Optional metadata about the requirement.
    public string? CompanyName { get; set; }
    public string? PpeRequired { get; set; }
    public int? ValidForDays { get; set; }

    // Current compliance status (enum).
    public ComplianceStatus Status { get; set; }

    // Readable display version of the status for UI.
    public string StatusDisplay => Status.ToString();

    // Dates related to the induction lifecycle.
    public DateTime IssueDateUtc { get; set; }
    public DateTime? CompletedOnUtc { get; set; }
    public DateTime? ExpiresOnUtc { get; set; }

    // Calculated field indicating how many days remain before expiry.
    public int? DaysLeft { get; set; }

    // Evidence file metadata (uploaded proof of completion).
    public string? EvidenceFileName { get; set; }
    public string? EvidenceContentType { get; set; }
    public string? EvidenceFilePath { get; set; }
    public DateTime? EvidenceUploadedOnUtc { get; set; }

    // Public or resolved URL for accessing the evidence file.
    public string? EvidenceUrl { get; set; }

    // Indicates whether evidence has been uploaded.
    public bool HasEvidence => !string.IsNullOrWhiteSpace(EvidenceFilePath);
}