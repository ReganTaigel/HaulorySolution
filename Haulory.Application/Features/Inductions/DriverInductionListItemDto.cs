using System;
using Haulory.Domain.Entities;

namespace Haulory.Application.Features.Incductions;

public class DriverInductionListItemDto
{
    public Guid DriverInductionId { get; set; }

    public Guid WorkSiteId { get; set; }
    public string WorkSiteName { get; set; } = string.Empty;

    public Guid RequirementId { get; set; }
    public string RequirementTitle { get; set; } = string.Empty;

    public string? CompanyName { get; set; }
    public string? PpeRequired { get; set; }
    public int? ValidForDays { get; set; }

    public ComplianceStatus Status { get; set; }

    public string StatusDisplay => Status.ToString();

    public DateTime IssueDateUtc { get; set; }
    public DateTime? CompletedOnUtc { get; set; }
    public DateTime? ExpiresOnUtc { get; set; }
    public int? DaysLeft { get; set; }

    public string? EvidenceFileName { get; set; }
    public string? EvidenceContentType { get; set; }
    public string? EvidenceFilePath { get; set; }
    public DateTime? EvidenceUploadedOnUtc { get; set; }
    public string? EvidenceUrl { get; set; }

    public bool HasEvidence => !string.IsNullOrWhiteSpace(EvidenceFilePath);
}