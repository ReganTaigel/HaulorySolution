namespace Haulory.Domain.Entities;

#region Enum: Driver Status

// Represents the lifecycle state of a Driver
public enum DriverStatus
{
    // Active driver available for jobs
    Active = 1,

    // Temporarily disabled (cannot be assigned jobs)
    Inactive = 2,

    // Soft-deleted / hidden from normal operations
    Archived = 3
}

#endregion
