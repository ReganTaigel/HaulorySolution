namespace Haulory.Application.Features.Vehicles.CreateVehicleSet;

public class CreateVehicleResult
{
    public bool Success { get; init; }
    public string? Message { get; init; }

    // Useful for navigation / future filtering
    public Guid? VehicleSetId { get; init; }
    public int AssetsCreated { get; init; }
}
