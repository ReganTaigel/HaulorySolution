using Haulory.Domain.Enums;

namespace Haulory.Domain.Entities;

public class VehicleAsset
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Links the assets created in one “Add Vehicle” wizard run
    public Guid VehicleSetId { get; set; } = Guid.NewGuid();

    public AssetKind Kind { get; set; }

    // PowerUnit only (Trailer can leave null)
    public VehicleType? VehicleType { get; set; }
    public FuelType? FuelType { get; set; }

    // Shared (PowerUnit + Trailer)
    public string Rego { get; set; } = string.Empty;
    public DateTime? RegoExpiry { get; set; }

    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;

    public ComplianceCertificateType CertificateType { get; set; } = ComplianceCertificateType.Wof;
    public DateTime? CertificateExpiry { get; set; }

    public int? OdometerKm { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    // Display helpers
    public string KindDisplay => Kind == AssetKind.PowerUnit ? "Vehicle" : "Trailer";

    public string VehicleTypeDisplay => VehicleType switch
    {
        Domain.Enums.VehicleType.Car => "Car",
        Domain.Enums.VehicleType.Ute => "Ute",
        Domain.Enums.VehicleType.LightVehicleTrailer => "Light VehicleTrailer",
        Domain.Enums.VehicleType.TruckClass2 => "Truck (Class 2)",
        Domain.Enums.VehicleType.TrailerClass3 => "Trailer (Class 3)",
        Domain.Enums.VehicleType.TruckClass4 => "Truck (Class 4)",
        Domain.Enums.VehicleType.TrailerClass5 => "Trailer (Class 5)",
        _ => VehicleType?.ToString() ?? string.Empty
    };

    public string FuelTypeDisplay => FuelType switch
    {
        Domain.Enums.FuelType.Petrol => "Petrol",
        Domain.Enums.FuelType.Diesel => "Diesel",
        Domain.Enums.FuelType.Electric => "Electric",
        Domain.Enums.FuelType.Hybrid => "Hybrid",
        _ => FuelType?.ToString() ?? string.Empty
    };

    public string CertificateNameDisplay => CertificateType == ComplianceCertificateType.Cof ? "COF" : "WOF";
}
