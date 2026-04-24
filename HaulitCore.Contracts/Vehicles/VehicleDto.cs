namespace HaulitCore.Contracts.Vehicles;

public class VehicleDto
{
    public Guid Id { get; set; }
    public Guid OwnerUserId { get; set; }
    public Guid VehicleSetId { get; set; }

    public string Rego { get; set; } = "";

    public string Make { get; set; } = "";
    public string Model { get; set; } = "";

    public int Year { get; set; }

    public int UnitNumber { get; set; }

    public string Kind { get; set; } = "";
    public string? VehicleType { get; set; }

    public string? FuelType { get; set; }
    public string? Configuration { get; set; }

    public DateTime? RegoExpiry { get; set; }

    public string? CertificateType { get; set; }
    public DateTime? CertificateExpiry { get; set; }

    public DateTime? RucPurchasedDate { get; set; }
    public int? RucDistancePurchasedKm { get; set; }

    public int? RucLicenceStartKm { get; set; }
    public int? RucLicenceEndKm { get; set; }

    public DateTime CreatedUtc { get; set; }

    public int? HubodometerKm { get; set; }
}