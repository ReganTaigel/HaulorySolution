namespace Haulory.Application.Features.Vehicles.CreateVehicleSet;

#region Result: Create Vehicle Set

public class CreateVehicleResult
{
    #region Status

    // Indicates whether the operation succeeded
    public bool Success { get; init; }

    // Optional message for UI feedback (error or success info)
    public string? Message { get; init; }

    #endregion

    #region Output Data

    // Identifier for the created vehicle set
    // Useful for navigation, filtering, or linking assets together
    public Guid? VehicleSetId { get; init; }

    // Number of assets created in this operation
    public int AssetsCreated { get; init; }

    #endregion
}

#endregion
