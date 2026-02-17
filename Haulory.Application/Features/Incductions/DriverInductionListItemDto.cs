using Haulory.Domain.Entities;

namespace Haulory.Application.Features.Incductions;

public class DriverInductionListItemDto
{
    public Guid DriverInductionId { get; set; }

    public Guid WorkSiteId { get; set; }
    public string WorkSiteName { get; set; } = string.Empty;

    public Guid RequirementId { get; set; }
    public string RequirementTitle { get; set; } = string.Empty;

    public int? ValidForDays { get; set; }
    public string? PpeRequired { get; set; }

    public ComplianceStatus Status { get; set; }
    public DateTime IssueDateUtc { get; set; }   // ✅ NEW
    public DateTime? CompletedOnUtc { get; set; }
    public DateTime? ExpiresOnUtc { get; set; }

    // ✅ NEW (optional but useful)
    public DateTime? NextDueUtc { get; set; }    // IssueDateUtc + ValidForDays
    public int? DaysLeft { get; set; }           // (NextDueUtc - UtcNow).Days
    public bool IsExpired => Status == ComplianceStatus.Expired;
    public bool IsCompleted => Status == ComplianceStatus.Completed;
    public bool IsInProgress => Status == ComplianceStatus.InProgress;
    public bool IsNotStarted => Status == ComplianceStatus.NotStarted;

    public string StatusDisplay => Status.ToString();


}
