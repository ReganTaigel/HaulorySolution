namespace Haulory.Api.Contracts.Vehicles;

public sealed class CreateVehicleUnitRequest
{
    public int UnitNumber { get; set; }

    public string Kind { get; set; } = string.Empty; // PowerUnit / Trailer
    public string VehicleType { get; set; } = string.Empty;

    public string Rego { get; set; } = string.Empty;
    public DateTime? RegoExpiry { get; set; }

    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }

    public string? FuelType { get; set; }
    public string? Configuration { get; set; }

    public string CertificateType { get; set; } = string.Empty; // Wof / Cof
    public DateTime? CertificateExpiry { get; set; }

    public int? OdometerKm { get; set; }

    public DateTime? RucPurchasedDate { get; set; }
    public int? RucDistancePurchasedKm { get; set; }
    public int? RucLicenceStartKm { get; set; }
    public int? RucLicenceEndKm { get; set; }
}