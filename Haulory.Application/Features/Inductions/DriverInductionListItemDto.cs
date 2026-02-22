using Haulory.Domain.Entities;

namespace Haulory.Application.Features.Incductions;

#region DTO: Driver Induction List Item

// Lightweight projection used for listing a driver's inductions.
// Designed for UI consumption (Mobile / API responses). 
// Contains:
// - Worksite + requirement metadata
// - Compliance status
// - Lifecycle dates
// - Calculated helper properties for UI display
public class DriverInductionListItemDto
{
    #region Identity

    // Unique identifier for the driver induction record.
    public Guid DriverInductionId { get; set; }

    // Associated worksite identifier.
    public Guid WorkSiteId { get; set; }

    // Display name of the worksite.
    public string WorkSiteName { get; set; } = string.Empty;

    // Requirement identifier.
    public Guid RequirementId { get; set; }

    // Title of the induction requirement.
    public string RequirementTitle { get; set; } = string.Empty;

    #endregion

    #region Requirement Configuration

    // Number of days the induction remains valid (if applicable).
    // Null means it does not expire.
    public int? ValidForDays { get; set; }

    // Personal protective equipment required for this induction.
    // Stored as simple comma-separated string (UI friendly).
    public string? PpeRequired { get; set; }

    #endregion

    #region Compliance Status

    // Current compliance state of the induction.
    public ComplianceStatus Status { get; set; }

    // Date the induction was issued (UTC).
    public DateTime IssueDateUtc { get; set; }

    // Date the induction was completed (if applicable).
    public DateTime? CompletedOnUtc { get; set; }

    // Expiry date of the induction (if applicable).
    public DateTime? ExpiresOnUtc { get; set; }

    #endregion

    #region Calculated / UI Helpers

    // Calculated next due date (typically IssueDateUtc + ValidForDays).
    // Optional because some inductions may not expire.
    public DateTime? NextDueUtc { get; set; }

    // Days remaining until expiry (based on current UTC time).
    // Null if not applicable.
    public int? DaysLeft { get; set; }

    // True if the induction is expired.
    public bool IsExpired => Status == ComplianceStatus.Expired;

    /// True if the induction has been completed.
    public bool IsCompleted => Status == ComplianceStatus.Completed;

    // True if the induction is currently in progress.
    public bool IsInProgress => Status == ComplianceStatus.InProgress;

    // True if the induction has not yet been started.
    public bool IsNotStarted => Status == ComplianceStatus.NotStarted;

    // String-friendly status representation for UI binding.
    public string StatusDisplay => Status.ToString();

    #endregion
}

#endregion
