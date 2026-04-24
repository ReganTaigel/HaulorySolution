using HaulitCore.Application.Features.Vehicles.UpdateVehicleSet;
using HaulitCore.Contracts.Vehicles;

namespace HaulitCore.Application.Features.Vehicles.UpdateVehicleSet;

public sealed class UpdateVehicleSetCommand
{
    // CHANGED:
    // Start from any asset id in the set being edited.
    public Guid VehicleAssetId { get; init; }

    // CHANGED:
    // Needed to keep edits inside the tenant boundary.
    public Guid OwnerUserId { get; init; }

    // CHANGED:
    // Full set payload from the mobile edit screen.
    public UpdateVehicleSetRequest Request { get; init; } = new();
}