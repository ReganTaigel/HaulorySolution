namespace Haulory.Application.Features.Vehicles.CreateVehicleSet;

#region Result: Create Vehicle Set

public class CreateVehicleResult
{
    #region Status

    public bool Success { get; init; }
    public string? Message { get; init; }

    #endregion

    #region Output Data

    public Guid? VehicleSetId { get; init; }
    public int AssetsCreated { get; init; }

    #endregion
}

#endregion