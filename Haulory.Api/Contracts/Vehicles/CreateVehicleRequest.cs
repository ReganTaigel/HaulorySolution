namespace Haulory.Api.Contracts.Vehicles;

public sealed class CreateVehicleRequest
{
    public string Rego { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }

    public int? UnitNumber { get; set; }

    public string? VehicleType { get; set; }
    public string? FuelType { get; set; }
    public string? Configuration { get; set; }

    public int? OdometerKm { get; set; }
}