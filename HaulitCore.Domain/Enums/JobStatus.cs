namespace HaulitCore.Domain.Enums;

public enum JobStatus
{
    Active = 1,                // assigned/in progress
    DeliveredPendingReview = 2, // driver completed but exceptions exist
    Completed = 3               // driver completed, no exceptions
}