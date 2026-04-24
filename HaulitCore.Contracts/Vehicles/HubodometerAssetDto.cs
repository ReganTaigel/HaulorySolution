using HaulitCore.Domain.Enums;

namespace HaulitCore.Contracts.Vehicles;

public sealed class HubodometerAssetDto
{
    public Guid Id { get; set; }
    public Guid VehicleSetId { get; set; }
    public int UnitNumber { get; set; }
    public string Rego { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int? HubodometerKm { get; set; }
}
