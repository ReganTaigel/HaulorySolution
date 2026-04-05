using Haulory.Domain.Entities;

namespace Haulory.Application.Features.Drivers;

// Represents the result of a driver creation operation.
// Encapsulates success status, optional error message, and the created driver entity.
public sealed class CreateDriverResult
{
    // Indicates whether the operation completed successfully.
    public bool Success { get; set; }

    // Contains an error message if the operation failed.
    public string? Error { get; set; }

    // The created driver entity when the operation succeeds.
    public Driver? Driver { get; set; }
}