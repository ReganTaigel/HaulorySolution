namespace HaulitCore.Contracts.Vehicles;

public sealed class CreateVehicleSetRequest
{
    public List<CreateVehicleUnitRequest> Units { get; set; } = new();
}