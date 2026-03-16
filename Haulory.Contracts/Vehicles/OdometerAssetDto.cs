using Haulory.Domain.Enums;

namespace Haulory.Contracts.Vehicles;

public sealed class OdometerAssetDto
{
    public Guid Id { get; set; }
    public Guid VehicleSetId { get; set; }
    public int UnitNumber { get; set; }
    public string Rego { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int? OdometerKm { get; set; }
}
