namespace HaulitCore.Contracts.Vehicles;

public sealed class UpdateVehicleSetRequest
{
    // Updating now works on the full set so edit matches create.
    public List<UpdateVehicleUnitRequest> Units { get; set; } = new();
}

public sealed class UpdateVehicleUnitRequest
{

    public Guid? Id { get; set; }

    // Expected values:
    // 1 = Power Unit
    // 2 = Trailer 1
    // 3 = Trailer 2 (B-train)
    public int UnitNumber { get; set; }

    public string Kind { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;

    public string Rego { get; set; } = string.Empty;
    public DateTime? RegoExpiry { get; set; }

    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }

    public string? FuelType { get; set; }
    public string? Configuration { get; set; }

    public string CertificateType { get; set; } = string.Empty;
    public DateTime? CertificateExpiry { get; set; }

    public int? HubodometerKm { get; set; }

    public DateTime? RucPurchasedDate { get; set; }
    public int? RucDistancePurchasedKm { get; set; }
    public int? RucLicenceStartKm { get; set; }
    public int? RucLicenceEndKm { get; set; }
}