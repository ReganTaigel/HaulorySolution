using Haulory.Domain.Entities;
using System.Collections.Generic;

namespace Haulory.Application.Features.Vehicles.CreateVehicleSet;

public class CreateVehicleCommand
{
    public Guid OwnerUserId { get; init; }
    public List<VehicleAsset> Assets { get; init; } = new();
}
