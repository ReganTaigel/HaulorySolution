using Haulory.Domain.Entities;

namespace Haulory.Application.Features.Vehicles.CreateVehicleSet;

#region Command: Create Vehicle Set

public class CreateVehicleCommand
{
    #region Ownership

    // The main account that owns this vehicle set
    public Guid OwnerUserId { get; init; }

    #endregion

    #region Vehicle Assets

    // Collection of vehicle assets to be created together
    // Example: Truck + Trailer combination
    public List<VehicleAsset> Assets { get; init; } = new();

    #endregion
}

#endregion
