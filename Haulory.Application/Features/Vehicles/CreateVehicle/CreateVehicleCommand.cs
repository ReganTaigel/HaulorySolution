using Haulory.Domain.Entities;
using System.Collections.Generic;

namespace Haulory.Application.Features.Vehicles.CreateVehicleSet;

public class CreateVehicleCommand
{
    // The wizard produces 1–3 assets (Unit 1/2/3)
    public List<VehicleAsset> Assets { get; init; } = new();
}
