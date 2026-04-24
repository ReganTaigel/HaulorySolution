namespace HaulitCore.Domain.Entities;

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

    // Odometer relevant for powered units and heavy trailers
    public int? HubodometerKm { get; set; }

    public DateTime? CertificateExpiry { get; set; }

    #endregion

    #region Behaviour

    public void SetOdometer(int km, bool allowDecrease = false)
    {
        if (km < 0)
            throw new ArgumentOutOfRangeException(nameof(km), "Hubodometer cannot be negative.");

        if (!allowDecrease && HubodometerKm.HasValue && km < HubodometerKm.Value)
            throw new InvalidOperationException("Hubodometer cannot be reduced unless a correction is being applied.");

        HubodometerKm = km;
    }

    #endregion
}

#endregion