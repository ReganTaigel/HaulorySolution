namespace Haulory.Api.Contracts.Vehicles;

public sealed class VehicleDto
{
    public Guid Id { get; set; }
    public Guid OwnerUserId { get; set; }
    public Guid VehicleSetId { get; set; }

    public string Rego { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }

    public int UnitNumber { get; set; }

    public string Kind { get; set; } = string.Empty;
    public string? VehicleType { get; set; }
    public string? FuelType { get; set; }
    public string? Configuration { get; set; }

    public int? OdometerKm { get; set; }
    public DateTime CreatedUtc { get; set; }
}