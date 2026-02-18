using System;
using Haulory.Domain.Enums;

namespace Haulory.Domain.Entities;

#region Entity: Vehicle Asset

public class VehicleAsset
{
    #region Identity / Ownership

    // Tenant boundary
    public Guid OwnerUserId { get; set; }

    public Guid Id { get; set; } = Guid.NewGuid();

    // Links assets created in one wizard run
    public Guid VehicleSetId { get; set; } = Guid.NewGuid();

    // Slot position:
    // 1 = Power unit
    // 2 = Trailer
    // 3 = Trailer 2 (B-Train)
    public int UnitNumber { get; set; }

    #endregion

    #region Classification

    public AssetKind Kind { get; set; }

    // Vehicle type (PowerUnit + Trailer)
    public VehicleType? VehicleType { get; set; }

    // PowerUnit only (Trailer can leave null)
    public FuelType? FuelType { get; set; }

    // Persisted configuration (trailers, and optional future use)
    public VehicleConfiguration? Configuration { get; set; }

    // Persist Class 4 subtype (Truck vs Tractor)
    public Class4PowerUnitType? Class4UnitType { get; set; }

    #endregion

    #region RUC (Road User Charges)

    // Applies to:
    // - Powered vehicle if FuelType is Diesel/Electric
    // - Heavy trailers (Class 3 and Class 5)
    // Stored per asset (Unit 1 / Unit 2 / Unit 3 each has its own RUC record)

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

    #endregion

    #region Core Vehicle Details

    public int Year { get; set; }

    public string Rego { get; set; } = string.Empty;
    public DateTime? RegoExpiry { get; set; }

    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;

    public ComplianceCertificateType CertificateType { get; set; } = ComplianceCertificateType.Wof;
    public DateTime? CertificateExpiry { get; set; }

    // Odometer applies to powered vehicles + heavy trailers
    public int? OdometerKm { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    #endregion

    #region Display Helpers

    public string KindDisplay => Kind == AssetKind.PowerUnit ? "Vehicle" : "Trailer";

    public string VehicleTypeDisplay => VehicleType switch
    {
        Enums.VehicleType.LightVehicle => "Light Vehicle",
        Enums.VehicleType.UtilityVehicle => "Utility Vehicle",
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

    // Trailer / config naming (KEEPING YOUR EDITS EXACTLY)
    public string ConfigurationDisplay => Configuration switch
    {
        // Light Trailer configs
        VehicleConfiguration.SingleAxle => "Single Axle Trailer",
        VehicleConfiguration.TandemAxle => "Tandem Axle Trailer",

        // Simi Trailer configs
        VehicleConfiguration.SemiCurtainsider => "Semi Trailer (Curtains)",
        VehicleConfiguration.SemiFlatDeck => "Semi Trailer (Flat Deck)",
        VehicleConfiguration.SemiRefrigerator => "Semi Trailer (Refrigerated)",
        VehicleConfiguration.SemiTanker => "Semi Trailer (Tanker)",
        VehicleConfiguration.SemiSkeleton => "Semi Trailer (Skeleton)",

        // Rigid Trucks configs
        VehicleConfiguration.RigidTruckCurtainsider => "Rigid Truck (Curtains)",
        VehicleConfiguration.RigidTruckFlatDeck => "Rigid Truck (Flat Deck)",
        VehicleConfiguration.RigidTruckRefrigerator => "Rigid Truck (Refrigerated)",
        VehicleConfiguration.RigidTruckTanker => "Rigid Truck (Tanker)",

        // Rigid Trailers
        VehicleConfiguration.CurtainSiderTrailer => "Curtainsider Trailer(A-Frame)",
        VehicleConfiguration.FlatDeckTrailer => "Flat Deck (A-Frame)",
        VehicleConfiguration.RefrigeratorTrailer => "Refrigerated Trailer (A-Frame)",
        VehicleConfiguration.TankerTrailer => "Tanker Trailer (A-Frame)",

        // B Train Trailers
        VehicleConfiguration.BCurtainSider => "Curtainsider Trailer (Fifth Wheel)",
        VehicleConfiguration.BFlatDeck => "Flat Deck Trailer (Fifth Wheel)",
        VehicleConfiguration.BRefigerator => "Refigerator Trailers (Fifth Wheel)",
        VehicleConfiguration.BTanker => "Tanker Trialer (Fifth Wheel)",

        // Tractor Units
        VehicleConfiguration.TractorUint => "Tractor Unit",

        _ => string.Empty
    };

    // Class 4 power unit subtype display
    public string PowerUnitSubtypeDisplay =>
        VehicleType == Enums.VehicleType.TruckClass4 && Class4UnitType != null
            ? (Class4UnitType == Enums.Class4PowerUnitType.Tractor ? "Tractor Unit" : "Truck")
            : string.Empty;

    // Prefix used by UI headers and cards
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

    #endregion

    #region Compliance: Rego + Certificate Status

    public bool IsRegoExpired =>
        RegoExpiry != null && RegoExpiry.Value.Date < DateTime.Today;

    public bool IsRegoDueSoon =>
        RegoExpiry != null && RegoExpiry.Value.Date <= DateTime.Today.AddDays(30);

    // Returns: "", "EXPIRED", or "DUE SOON"
    public string RegoStatusDisplay =>
        RegoExpiry == null ? string.Empty :
        IsRegoExpired ? "EXPIRED" :
        IsRegoDueSoon ? "DUE SOON" :
        string.Empty;

    // Returns: "", " • EXPIRED", or " • DUE SOON"
    public string RegoStatusInlineDisplay =>
        string.IsNullOrWhiteSpace(RegoStatusDisplay) ? string.Empty : $" • {RegoStatusDisplay}";

    // Certificate status (your original logic)
    public bool IsCertExpired =>
        CertificateExpiry != null && CertificateExpiry.Value.Date < DateTime.Today;

    public bool IsCertDueSoon =>
        CertificateExpiry != null && CertificateExpiry.Value.Date <= DateTime.Today.AddDays(30);

    // Returns: "", "EXPIRED", or "DUE SOON"
    public string CertStatusDisplay =>
        CertificateExpiry == null ? string.Empty :
        IsCertExpired ? "EXPIRED" :
        IsCertDueSoon ? "DUE SOON" :
        string.Empty;

    // Returns: "", " • EXPIRED", or " • DUE SOON"
    public string CertStatusInlineDisplay =>
        string.IsNullOrWhiteSpace(CertStatusDisplay) ? string.Empty : $" • {CertStatusDisplay}";

    #endregion

    #region RUC Display Helpers

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

    // Purchase metadata (analytics)
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

    // YES/NO string for UI
    public string IsRucOverdueYesNo => IsRucOverdue ? "YES" : "NO";

    #endregion
}

#endregion
