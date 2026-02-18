using Haulory.Domain.Entities;

namespace Haulory.Application.Features.Incductions;

#region DTO: Driver Induction List Item

/// <summary>
/// Lightweight projection used for listing a driver's inductions.
/// Designed for UI consumption (Mobile / API responses).
/// 
/// Contains:
/// - Worksite + requirement metadata
/// - Compliance status
/// - Lifecycle dates
/// - Calculated helper properties for UI display
/// </summary>
public class DriverInductionListItemDto
{
    #region Identity

    /// <summary>
    /// Unique identifier for the driver induction record.
    /// </summary>
    public Guid DriverInductionId { get; set; }

    /// <summary>
    /// Associated worksite identifier.
    /// </summary>
    public Guid WorkSiteId { get; set; }

    /// <summary>
    /// Display name of the worksite.
    /// </summary>
    public string WorkSiteName { get; set; } = string.Empty;

    /// <summary>
    /// Requirement identifier.
    /// </summary>
    public Guid RequirementId { get; set; }

    /// <summary>
    /// Title of the induction requirement.
    /// </summary>
    public string RequirementTitle { get; set; } = string.Empty;

    #endregion

    #region Requirement Configuration

    /// <summary>
    /// Number of days the induction remains valid (if applicable).
    /// Null means it does not expire.
    /// </summary>
    public int? ValidForDays { get; set; }

    /// <summary>
    /// Personal protective equipment required for this induction.
    /// Stored as simple comma-separated string (UI friendly).
    /// </summary>
    public string? PpeRequired { get; set; }

    #endregion

    #region Compliance Status

    /// <summary>
    /// Current compliance state of the induction.
    /// </summary>
    public ComplianceStatus Status { get; set; }

    /// <summary>
    /// Date the induction was issued (UTC).
    /// </summary>
    public DateTime IssueDateUtc { get; set; }

    /// <summary>
    /// Date the induction was completed (if applicable).
    /// </summary>
    public DateTime? CompletedOnUtc { get; set; }

    /// <summary>
    /// Expiry date of the induction (if applicable).
    /// </summary>
    public DateTime? ExpiresOnUtc { get; set; }

    #endregion

    #region Calculated / UI Helpers

    /// <summary>
    /// Calculated next due date (typically IssueDateUtc + ValidForDays).
    /// Optional because some inductions may not expire.
    /// </summary>
    public DateTime? NextDueUtc { get; set; }

    /// <summary>
    /// Days remaining until expiry (based on current UTC time).
    /// Null if not applicable.
    /// </summary>
    public int? DaysLeft { get; set; }

    /// <summary>
    /// True if the induction is expired.
    /// </summary>
    public bool IsExpired => Status == ComplianceStatus.Expired;

    /// <summary>
    /// True if the induction has been completed.
    /// </summary>
    public bool IsCompleted => Status == ComplianceStatus.Completed;

    /// <summary>
    /// True if the induction is currently in progress.
    /// </summary>
    public bool IsInProgress => Status == ComplianceStatus.InProgress;

    /// <summary>
    /// True if the induction has not yet been started.
    /// </summary>
    public bool IsNotStarted => Status == ComplianceStatus.NotStarted;

    /// <summary>
    /// String-friendly status representation for UI binding.
    /// </summary>
    public string StatusDisplay => Status.ToString();

    #endregion
}

#endregion
