using System.Collections.ObjectModel;
using Haulory.Domain.Enums;

namespace Haulory.Moblie.ViewModels;

public class NewVehicleViewModel : BaseViewModel
{
    #region Fields
    // Selection
    private VehicleOption<VehicleType>? _selectedVehicleType;
    private VehicleOption<VehicleConfiguration>? _selectedLightConfig;
    private VehicleOption<VehicleConfiguration>? _selectedHeavyConfig;
    private VehicleOption<Class4PowerUnitType>? _selectedClass4UnitType;
    private VehicleOption<FuelType>? _selectedFuelType;

    // Rego
    private string _unit1Rego = string.Empty;
    private string _unit2Rego = string.Empty;
    private string _unit3Rego = string.Empty;

    // Rego Expiry
    private DateTime? _unit1RegoExpiry;
    private DateTime? _unit2RegoExpiry;
    private DateTime? _unit3RegoExpiry;

    // Odo Numbers
    private int? _powerUnitOdometerKm;
    private int? _trailer1OdometerKm;
    private int? _trailer2OdometerKm;

    // COF/WOF Expiry date (per unit)
    private DateTime? _unit1CertExpiry;
    private DateTime? _unit2CertExpiry;
    private DateTime? _unit3CertExpiry;
    #endregion

    #region Pickers (Options)

    // Vehicle selection type
    public ObservableCollection<VehicleOption<VehicleType>> VehicleTypes { get; } =
        new()
        {
            new(VehicleType.Car, "Car"),
            new(VehicleType.CarWithTrailer, "Car + Trailer"),
            new(VehicleType.Ute, "Ute"),
            new(VehicleType.UteWithTrailer, "Ute + Trailer"),

            new(VehicleType.TruckClass2, "Truck (Class 2)"),
            new(VehicleType.TruckClass2AndTrailerClass3, "Truck + Trailer (Class 2/3)"),

            new(VehicleType.TruckClass4, "Truck (Class 4)"),
            new(VehicleType.TruckClass4AndTrailerClass5, "Truck (Class 4) + Trailer (Class 5)")
        };

    // Light trailer config
    public ObservableCollection<VehicleOption<VehicleConfiguration>> LightTrailerConfigurations { get; } =
        new()
        {
            new(VehicleConfiguration.SingleAxle, "Single axle trailer"),
            new(VehicleConfiguration.TandemAxle, "Tandem axle trailer"),
        };

    // Only used for class 4+5 (because config matters there)
    public ObservableCollection<VehicleOption<VehicleConfiguration>> HeavyConfigurations { get; } =
        new()
        {
            new(VehicleConfiguration.TruckAndTrailer, "Truck + Trailer"),
            new(VehicleConfiguration.BTrain, "B-Train (tractor + 2 trailers)")
        };

    // Class 4 should only ask Truck vs Tractor
    public ObservableCollection<VehicleOption<Class4PowerUnitType>> Class4UnitTypes { get; } =
        new()
        {
            new(Class4PowerUnitType.Truck, "Truck"),
            new(Class4PowerUnitType.Tractor, "Tractor")
        };

    // Fuel selection type
    public ObservableCollection<VehicleOption<FuelType>> FuelTypes { get; } =
        new()
        {
            new(FuelType.Petrol, "Petrol"),
            new(FuelType.Diesel, "Diesel"),
            new(FuelType.Electric, "Electric"),
            new(FuelType.Hybrid, "Hybrid")
        };

    #endregion

    #region Selected Values

    public VehicleOption<VehicleType>? SelectedVehicleType
    {
        get => _selectedVehicleType;
        set
        {
            _selectedVehicleType = value;

            // reset dependent selections when type changes
            SelectedLightConfig = null;
            SelectedHeavyConfig = null;
            SelectedClass4UnitType = null;
            SelectedFuelType = null;

            // clear regos
            Unit1Rego = string.Empty;
            Unit2Rego = string.Empty;
            Unit3Rego = string.Empty;

            // clear rego expiries
            Unit1RegoExpiry = null;
            Unit2RegoExpiry = null;
            Unit3RegoExpiry = null;

            // clear cert expiries
            Unit1CertExpiry = null;
            Unit2CertExpiry = null;
            Unit3CertExpiry = null;

            // clear odos
            PowerUnitOdometerKm = null;
            Trailer1OdometerKm = null;
            Trailer2OdometerKm = null;

            OnPropertyChanged();
            RaiseAllVisibility();
        }
    }

    public VehicleOption<VehicleConfiguration>? SelectedLightConfig
    {
        get => _selectedLightConfig;
        set
        {
            _selectedLightConfig = value;

            // trailer config changes can change rego count logic, so clear secondary values
            Unit2Rego = string.Empty;
            Unit3Rego = string.Empty;

            Unit2RegoExpiry = null;
            Unit3RegoExpiry = null;

            Unit2CertExpiry = null;
            Unit3CertExpiry = null;

            SelectedFuelType = null;

            PowerUnitOdometerKm = null;
            Trailer1OdometerKm = null;
            Trailer2OdometerKm = null;

            OnPropertyChanged();
            RaiseAllVisibility();
        }
    }

    public VehicleOption<Class4PowerUnitType>? SelectedClass4UnitType
    {
        get => _selectedClass4UnitType;
        set
        {
            _selectedClass4UnitType = value;

            // changing unit type should reset downstream flow
            SelectedFuelType = null;

            Unit1RegoExpiry = null;
            Unit2RegoExpiry = null;
            Unit3RegoExpiry = null;

            Unit1CertExpiry = null;
            Unit2CertExpiry = null;
            Unit3CertExpiry = null;

            PowerUnitOdometerKm = null;
            Trailer1OdometerKm = null;
            Trailer2OdometerKm = null;

            OnPropertyChanged();
            RaiseAllVisibility();
        }
    }

    public VehicleOption<VehicleConfiguration>? SelectedHeavyConfig
    {
        get => _selectedHeavyConfig;
        set
        {
            _selectedHeavyConfig = value;

            // heavy config changes can switch between 2 and 3 units, reset downstream fields
            Unit2Rego = string.Empty;
            Unit3Rego = string.Empty;

            Unit2RegoExpiry = null;
            Unit3RegoExpiry = null;

            Unit2CertExpiry = null;
            Unit3CertExpiry = null;

            SelectedFuelType = null;

            PowerUnitOdometerKm = null;
            Trailer1OdometerKm = null;
            Trailer2OdometerKm = null;

            OnPropertyChanged();
            RaiseAllVisibility();
        }
    }

    public VehicleOption<FuelType>? SelectedFuelType
    {
        get => _selectedFuelType;
        set
        {
            _selectedFuelType = value;

            // when fuel changes, clear odos
            PowerUnitOdometerKm = null;
            Trailer1OdometerKm = null;
            Trailer2OdometerKm = null;

            OnPropertyChanged();
            OnPropertyChanged(nameof(RequiresRuc));
            OnPropertyChanged(nameof(FuelInfoText));

            RaiseAllVisibility();
        }
    }

    #endregion

    #region Rego Fields (Unit 1/2/3)

    public string Unit1Rego
    {
        get => _unit1Rego;
        set { _unit1Rego = value; OnPropertyChanged(); RaiseAllVisibility(); }
    }

    public string Unit2Rego
    {
        get => _unit2Rego;
        set { _unit2Rego = value; OnPropertyChanged(); RaiseAllVisibility(); }
    }

    public string Unit3Rego
    {
        get => _unit3Rego;
        set { _unit3Rego = value; OnPropertyChanged(); RaiseAllVisibility(); }
    }

    #endregion

    #region Rego Expiry

    public DateTime? Unit1RegoExpiry
    {
        get => _unit1RegoExpiry;
        set { _unit1RegoExpiry = value; OnPropertyChanged(); RaiseAllVisibility(); }
    }

    public DateTime? Unit2RegoExpiry
    {
        get => _unit2RegoExpiry;
        set { _unit2RegoExpiry = value; OnPropertyChanged(); RaiseAllVisibility(); }
    }

    public DateTime? Unit3RegoExpiry
    {
        get => _unit3RegoExpiry;
        set { _unit3RegoExpiry = value; OnPropertyChanged(); RaiseAllVisibility(); }
    }

    #endregion

    #region COF/WOF Expiry (per unit)

    public DateTime? Unit1CertExpiry
    {
        get => _unit1CertExpiry;
        set { _unit1CertExpiry = value; OnPropertyChanged(); RaiseAllVisibility(); }
    }

    public DateTime? Unit2CertExpiry
    {
        get => _unit2CertExpiry;
        set { _unit2CertExpiry = value; OnPropertyChanged(); RaiseAllVisibility(); }
    }

    public DateTime? Unit3CertExpiry
    {
        get => _unit3CertExpiry;
        set { _unit3CertExpiry = value; OnPropertyChanged(); RaiseAllVisibility(); }
    }

    #endregion

    #region Odometer Fields

    public int? PowerUnitOdometerKm
    {
        get => _powerUnitOdometerKm;
        set { _powerUnitOdometerKm = value; OnPropertyChanged(); }
    }

    public int? Trailer1OdometerKm
    {
        get => _trailer1OdometerKm;
        set { _trailer1OdometerKm = value; OnPropertyChanged(); }
    }

    public int? Trailer2OdometerKm
    {
        get => _trailer2OdometerKm;
        set { _trailer2OdometerKm = value; OnPropertyChanged(); }
    }

    #endregion

    #region Flow / Visibility

    public bool HasVehicleType => SelectedVehicleType != null;

    public bool ShowLightTrailerConfig =>
        SelectedVehicleType?.Value == VehicleType.CarWithTrailer ||
        SelectedVehicleType?.Value == VehicleType.UteWithTrailer;

    public bool ShowClass4UnitType =>
        SelectedVehicleType?.Value == VehicleType.TruckClass4;

    public bool ShowHeavyConfig =>
        SelectedVehicleType?.Value == VehicleType.TruckClass4AndTrailerClass5;

    public bool ReadyForRego =>
        HasVehicleType &&
        (!ShowLightTrailerConfig || SelectedLightConfig != null) &&
        (!ShowClass4UnitType || SelectedClass4UnitType != null) &&
        (!ShowHeavyConfig || SelectedHeavyConfig != null);

    public bool IsHeavyVehicle =>
        HasVehicleType &&
        (SelectedVehicleType!.Value == VehicleType.TruckClass2 ||
         SelectedVehicleType.Value == VehicleType.TruckClass2AndTrailerClass3 ||
         SelectedVehicleType.Value == VehicleType.TruckClass4 ||
         SelectedVehicleType.Value == VehicleType.TruckClass4AndTrailerClass5);

    // WOF for light, COF for heavy
    public ComplianceCertificateType RequiredCertificate =>
        IsHeavyVehicle ? ComplianceCertificateType.Cof : ComplianceCertificateType.Wof;

    public string CertificateName => RequiredCertificate == ComplianceCertificateType.Cof ? "COF" : "WOF";
    public string CertificatePluralName => RequiredCertificate == ComplianceCertificateType.Cof ? "COFs" : "WOFs";

    public bool RegoComplete
    {
        get
        {
            if (!ReadyForRego) return false;

            if (string.IsNullOrWhiteSpace(Unit1Rego)) return false;
            if (RegoCount >= 2 && string.IsNullOrWhiteSpace(Unit2Rego)) return false;
            if (RegoCount >= 3 && string.IsNullOrWhiteSpace(Unit3Rego)) return false;

            return true;
        }
    }

    public bool ShowRegoExpiry => ReadyForRego && RegoComplete;

    public bool RegoExpiryComplete
    {
        get
        {
            if (!ShowRegoExpiry) return false;

            if (Unit1RegoExpiry == null) return false;
            if (RegoCount >= 2 && Unit2RegoExpiry == null) return false;
            if (RegoCount >= 3 && Unit3RegoExpiry == null) return false;

            return true;
        }
    }

    // show COF/WOF after rego expiry is complete
    public bool ShowCertificateStep => RegoExpiryComplete;

    // number of certificates required matches unit count rules:
    // car=1, car+trailer=2, class4=1, class4+5=2, b-train=3
    public int CertificateCount => RegoCount;

    public bool ShowUnit1Cert => ShowCertificateStep;
    public bool ShowUnit2Cert => ShowCertificateStep && CertificateCount >= 2;
    public bool ShowUnit3Cert => ShowCertificateStep && CertificateCount >= 3;

    public string Unit1CertLabel => $"{Unit1Label} {CertificateName} expiry";
    public string Unit2CertLabel => $"{Unit2Label} {CertificateName} expiry";
    public string Unit3CertLabel => $"{Unit3Label} {CertificateName} expiry";

    public bool CertificateComplete
    {
        get
        {
            if (!ShowCertificateStep) return false;

            if (Unit1CertExpiry == null) return false;
            if (CertificateCount >= 2 && Unit2CertExpiry == null) return false;
            if (CertificateCount >= 3 && Unit3CertExpiry == null) return false;

            return true;
        }
    }

    // fuel shows after certificate complete
    public bool ShowFuelType => CertificateComplete;

    // odo shows after fuel chosen
    public bool ShowOdometers => ShowFuelType && SelectedFuelType != null;

    public int RegoCount
    {
        get
        {
            if (!HasVehicleType) return 0;

            if (SelectedVehicleType!.Value == VehicleType.Car ||
                SelectedVehicleType.Value == VehicleType.Ute ||
                SelectedVehicleType.Value == VehicleType.TruckClass2 ||
                SelectedVehicleType.Value == VehicleType.TruckClass4)
                return 1;

            if (SelectedVehicleType.Value == VehicleType.CarWithTrailer ||
                SelectedVehicleType.Value == VehicleType.UteWithTrailer ||
                SelectedVehicleType.Value == VehicleType.TruckClass2AndTrailerClass3 ||
                SelectedVehicleType.Value == VehicleType.TruckClass4AndTrailerClass5)
            {
                if (SelectedVehicleType.Value == VehicleType.TruckClass4AndTrailerClass5 &&
                    SelectedHeavyConfig?.Value == VehicleConfiguration.BTrain)
                    return 3;

                return 2;
            }

            return 1;
        }
    }

    public bool ShowUnit1Rego => ReadyForRego;
    public bool ShowUnit2Rego => ReadyForRego && RegoCount >= 2;
    public bool ShowUnit3Rego => ReadyForRego && RegoCount >= 3;

    public string Unit1Label => RegoCount >= 1 ? "Vehicle rego" : string.Empty;
    public string Unit2Label => RegoCount >= 2 ? "Trailer rego" : string.Empty;
    public string Unit3Label => RegoCount >= 3 ? "Trailer 2 rego" : string.Empty;

    // ODO rules:
    // - Car/Ute (with or without trailer) = 1 odo
    // - Truck class 2/4 = 1 odo
    // - Truck+Trailer = 2 odos
    // - B-train = 3 odos
    public int OdoCount
    {
        get
        {
            if (!HasVehicleType) return 0;

            if (SelectedVehicleType!.Value == VehicleType.Car ||
                SelectedVehicleType.Value == VehicleType.Ute ||
                SelectedVehicleType.Value == VehicleType.CarWithTrailer ||
                SelectedVehicleType.Value == VehicleType.UteWithTrailer)
                return 1;

            if (SelectedVehicleType.Value == VehicleType.TruckClass2 ||
                SelectedVehicleType.Value == VehicleType.TruckClass4)
                return 1;

            if (SelectedVehicleType.Value == VehicleType.TruckClass2AndTrailerClass3)
                return 2;

            if (SelectedVehicleType.Value == VehicleType.TruckClass4AndTrailerClass5)
            {
                if (SelectedHeavyConfig?.Value == VehicleConfiguration.BTrain)
                    return 3;
                return 2;
            }

            return 1;
        }
    }

    public bool ShowPowerUnitOdo => ShowOdometers && OdoCount >= 1;
    public bool ShowTrailer1Odo => ShowOdometers && OdoCount >= 2;
    public bool ShowTrailer2Odo => ShowOdometers && OdoCount >= 3;

    public string PowerUnitOdoLabel => "Odometer (km)";
    public string Trailer1OdoLabel => "Trailer odometer (km)";
    public string Trailer2OdoLabel => "Trailer 2 odometer (km)";

    private void RaiseAllVisibility()
    {
        OnPropertyChanged(nameof(HasVehicleType));
        OnPropertyChanged(nameof(ShowLightTrailerConfig));
        OnPropertyChanged(nameof(ShowClass4UnitType));
        OnPropertyChanged(nameof(ShowHeavyConfig));

        OnPropertyChanged(nameof(ReadyForRego));
        OnPropertyChanged(nameof(IsHeavyVehicle));

        OnPropertyChanged(nameof(RequiredCertificate));
        OnPropertyChanged(nameof(CertificateName));
        OnPropertyChanged(nameof(CertificatePluralName));

        OnPropertyChanged(nameof(RegoCount));
        OnPropertyChanged(nameof(ShowUnit1Rego));
        OnPropertyChanged(nameof(ShowUnit2Rego));
        OnPropertyChanged(nameof(ShowUnit3Rego));

        OnPropertyChanged(nameof(Unit1Label));
        OnPropertyChanged(nameof(Unit2Label));
        OnPropertyChanged(nameof(Unit3Label));

        OnPropertyChanged(nameof(RegoComplete));
        OnPropertyChanged(nameof(ShowRegoExpiry));
        OnPropertyChanged(nameof(RegoExpiryComplete));

        OnPropertyChanged(nameof(ShowCertificateStep));
        OnPropertyChanged(nameof(CertificateCount));
        OnPropertyChanged(nameof(ShowUnit1Cert));
        OnPropertyChanged(nameof(ShowUnit2Cert));
        OnPropertyChanged(nameof(ShowUnit3Cert));
        OnPropertyChanged(nameof(Unit1CertLabel));
        OnPropertyChanged(nameof(Unit2CertLabel));
        OnPropertyChanged(nameof(Unit3CertLabel));
        OnPropertyChanged(nameof(CertificateComplete));

        OnPropertyChanged(nameof(ShowFuelType));

        OnPropertyChanged(nameof(ShowOdometers));
        OnPropertyChanged(nameof(OdoCount));
        OnPropertyChanged(nameof(ShowPowerUnitOdo));
        OnPropertyChanged(nameof(ShowTrailer1Odo));
        OnPropertyChanged(nameof(ShowTrailer2Odo));
        OnPropertyChanged(nameof(PowerUnitOdoLabel));
        OnPropertyChanged(nameof(Trailer1OdoLabel));
        OnPropertyChanged(nameof(Trailer2OdoLabel));
    }

    #endregion

    #region Fuel Logic

    public bool RequiresRuc =>
        SelectedFuelType?.Value == FuelType.Diesel ||
        SelectedFuelType?.Value == FuelType.Electric;

    public string FuelInfoText =>
        SelectedFuelType == null
            ? string.Empty
            : RequiresRuc
                ? "Road User Charges (RUC) apply to this vehicle."
                : "No Road User Charges (RUC) required for this fuel type.";

    #endregion
}
