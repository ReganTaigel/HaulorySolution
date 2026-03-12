using Haulory.Domain.Entities;

namespace Haulory.Application.Features.Drivers;

public sealed class CreateDriverResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public Driver? Driver { get; set; }
}