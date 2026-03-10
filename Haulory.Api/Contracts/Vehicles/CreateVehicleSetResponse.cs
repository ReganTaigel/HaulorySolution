namespace Haulory.Api.Contracts.Vehicles;

public sealed class CreateVehicleSetResponse
{
    public string Message { get; set; } = string.Empty;
    public Guid VehicleSetId { get; set; }
    public int AssetsCreated { get; set; }
    public List<VehicleDto> Assets { get; set; } = new();
}