namespace Haulory.Domain.Entities;

#region Entity: Vehicle Unit (Lightweight Slot Model)

public class VehicleUnit
{
    #region Slot Identity

    // Slot position in a vehicle set
    // 1 = Power unit
    // 2 = Trailer 1
    // 3 = Trailer 2 (B-Train)
    public int UnitNumber { get; set; }

    #endregion

    #region Core Details

    public int Year { get; set; }

    public string Rego { get; set; } = string.Empty;
    public DateTime? RegoExpiry { get; set; }

    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;

    #endregion

    #region Compliance

    public DateTime? CertificateExpiry { get; set; }

    // Odometer relevant for powered units and heavy trailers
    public int? OdometerKm { get; set; }

    #endregion
}

#endregion
