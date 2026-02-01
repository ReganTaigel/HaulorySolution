using Haulory.Domain.Enums;

namespace Haulory.Domain.Entities;

public class VehicleAsset
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Links assets created in one wizard run
    public Guid VehicleSetId { get; set; } = Guid.NewGuid();

    // SLOT POSITION
    // 1 = Power unit
    // 2 = Trailer
    // 3 = Trailer 2 (B-Train)
    public int UnitNumber { get; set; }

    public AssetKind Kind { get; set; }

    // Vehicle type (PowerUnit + Trailer)
    public VehicleType? VehicleType { get; set; }

    // PowerUnit only (Trailer can leave null)
    public FuelType? FuelType { get; set; }

    // Persisted configuration (trailers, and optional future use)
    public VehicleConfiguration? Configuration { get; set; }

    // Persist Class 4 subtype (Truck vs Tractor)
    public Class4PowerUnitType? Class4UnitType { get; set; }

    // ------------------------
    // RUC (Road User Charges) fields
    // Applies to:
    // - Powered vehicle if FuelType is Diesel/Electric
    // - Heavy trailers (Class 3 and Class 5)
    // Stored PER ASSET (so Unit 1 / Unit 2 / Unit 3 each has its own RUC record)
    // ------------------------

    // Odometer reading when the latest RUC distance was purchased
    public int? RucOdometerAtPurchaseKm { get; set; }

    // How many km were purchased on that RUC transaction
    public int? RucDistancePurchasedKm { get; set; }

    // When the RUC was purchased
    public DateTime? RucPurchasedDate { get; set; }

    // Convenience: at what odometer the next RUC top-up is due
    // (usually RucOdometerAtPurchaseKm + RucDistancePurchasedKm)
    public int? RucNextDueOdometerKm { get; set; }

    // RUC licence range (the current/last licence bought)
    public int? RucLicenceStartKm { get; set; }   // start odometer on licence (e.g. 1000)
    public int? RucLicenceEndKm { get; set; }     // end odometer on licence (e.g. 2000)


    // ------------------------
    // Shared
    // ------------------------
    public int Year { get; set; }

    public string Rego { get; set; } = string.Empty;
    public DateTime? RegoExpiry { get; set; }

    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;

    public ComplianceCertificateType CertificateType { get; set; } = ComplianceCertificateType.Wof;
    public DateTime? CertificateExpiry { get; set; }

    // Odo only for powered + heavy trailers
    public int? OdometerKm { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    // ------------------------
    // Display helpers
    // ------------------------

    public string KindDisplay => Kind == AssetKind.PowerUnit ? "Vehicle" : "Trailer";

    public string VehicleTypeDisplay => VehicleType switch
    {
        Enums.VehicleType.Car => "Car",
        Enums.VehicleType.Ute => "Ute",
        Enums.VehicleType.LightVehicleTrailer => "Light Vehicle Trailer",
        Enums.VehicleType.TruckClass2 => "Truck (Class 2)",
        Enums.VehicleType.TrailerClass3 => "Trailer (Class 3)",
        Enums.VehicleType.TruckClass4 => "Truck (Class 4)",
        Enums.VehicleType.TrailerClass5 => "Trailer (Class 5)",
        _ => VehicleType?.ToString() ?? string.Empty
    };

    public string FuelTypeDisplay => FuelType switch
    {
        Enums.FuelType.Petrol => "Petrol",
        Enums.FuelType.Diesel => "Diesel",
        Enums.FuelType.Electric => "Electric",
        Enums.FuelType.Hybrid => "Hybrid",
        _ => FuelType?.ToString() ?? string.Empty
    };

    public string CertificateNameDisplay =>
        CertificateType == ComplianceCertificateType.Cof ? "COF" : "WOF";

    // Unit slot labels (slot rules)
    public string UnitRoleDisplay => UnitNumber switch
    {
        1 => "Vehicle",
        2 => "Trailer 1",
        3 => "Trailer 2",
        _ => $"Unit {UnitNumber}"
    };

    // Backwards-compat alias
    public string UnitDisplay => UnitRoleDisplay;

    // Trailer / config naming
    public string ConfigurationDisplay => Configuration switch
    {
        VehicleConfiguration.SingleAxle => "Single Axle Trailer",
        VehicleConfiguration.TandemAxle => "Tandem Axle Trailer",

        VehicleConfiguration.SemiFlatDeck => "Semi Flat Deck Trailer",
        VehicleConfiguration.SemiSkeleton => "Semi Skeleton Trailer",
        VehicleConfiguration.SemiRefrigerator => "Semi Refrigerated Trailer",

        VehicleConfiguration.CurtainSiderTrailer => "Curtainsider Trailer",
        VehicleConfiguration.RefrigeratorTrailer => "Refrigerated Trailer",

        VehicleConfiguration.BTrainCurtainSider => "Curtainsider Trailer",

        VehicleConfiguration.Rigid => "Rigid Truck",
        VehicleConfiguration.RigidCold => "Rigid Refrigerated Truck",
        VehicleConfiguration.TractorUint => "Tractor Unit",

        _ => string.Empty
    };

    // Class 4 power unit subtype display
    public string PowerUnitSubtypeDisplay =>
        VehicleType == Enums.VehicleType.TruckClass4 && Class4UnitType != null
            ? (Class4UnitType == Enums.Class4PowerUnitType.Tractor ? "Tractor Unit" : "Truck")
            : string.Empty;

    public string TitlePrefix
    {
        get
        {
            if (Kind == AssetKind.PowerUnit)
            {
                var subtype = PowerUnitSubtypeDisplay;
                return string.IsNullOrWhiteSpace(subtype)
                    ? UnitRoleDisplay
                    : $"{UnitRoleDisplay} • {subtype}";
            }

            var config = ConfigurationDisplay;
            return string.IsNullOrWhiteSpace(config)
                ? UnitRoleDisplay
                : $"{UnitRoleDisplay} • {config}";
        }
    }

    // Safe date strings for UI
    public string RegoExpiryDisplay => RegoExpiry?.ToString("dd/MM/yyyy") ?? "—";
    public string CertificateExpiryDisplay => CertificateExpiry?.ToString("dd/MM/yyyy") ?? "—";

    // Compliance status helpers (optional UI badges)
    public bool IsCertExpired =>
        CertificateExpiry != null && CertificateExpiry.Value.Date < DateTime.Today;

    public bool IsCertDueSoon =>
        CertificateExpiry != null && CertificateExpiry.Value.Date <= DateTime.Today.AddDays(30);

    public string CertStatusDisplay =>
        CertificateExpiry == null ? string.Empty :
        IsCertExpired ? " • EXPIRED" :
        IsCertDueSoon ? " • DUE SOON" :
        string.Empty;

    // ------------------------
    // RUC display helpers
    // ------------------------

    // RUC applies to:
    // - Power unit where fuel is Diesel/Electric
    // - Heavy trailers: Trailer Class 3 and Trailer Class 5
    public bool IsRucApplicable
    {
        get
        {
            // Heavy trailers
            if (VehicleType == Enums.VehicleType.TrailerClass3 ||
                VehicleType == Enums.VehicleType.TrailerClass5)
                return true;

            // Powered vehicles (fuel-based)
            if (Kind == AssetKind.PowerUnit &&
                (FuelType == Enums.FuelType.Diesel || FuelType == Enums.FuelType.Electric))
                return true;

            return false;
        }
    }

    // Display for purchase metadata (analytics)
    public string RucPurchasedDateDisplay =>
        RucPurchasedDate?.ToString("dd/MM/yyyy") ?? "—";

    public string RucDistancePurchasedDisplay =>
        RucDistancePurchasedKm == null ? "—" : $"{RucDistancePurchasedKm.Value:N0} km";

    // Licence range display (compliance)
    public string RucLicenceStartDisplay =>
        RucLicenceStartKm == null ? "—" : $"{RucLicenceStartKm.Value:N0} km";

    public string RucLicenceEndDisplay =>
        RucLicenceEndKm == null ? "—" : $"{RucLicenceEndKm.Value:N0} km";

    // Remaining against current odometer (compliance)
    public int? RucKmRemaining =>
        (IsRucApplicable && OdometerKm != null && RucLicenceEndKm != null)
            ? Math.Max(0, RucLicenceEndKm.Value - OdometerKm.Value)
            : null;

    public string RucRemainingDisplay =>
        RucKmRemaining == null ? "—" : $"{RucKmRemaining.Value:N0} km";

    public bool IsRucOverdue =>
        IsRucApplicable && OdometerKm != null && RucLicenceEndKm != null
        && OdometerKm.Value > RucLicenceEndKm.Value;
}
