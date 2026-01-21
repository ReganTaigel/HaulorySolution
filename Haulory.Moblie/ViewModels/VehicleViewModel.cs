using System.Collections.ObjectModel;
using Haulory.Domain.Enums;

namespace Haulory.Moblie.ViewModels;

public class VehicleViewModel : BaseViewModel
{
    #region Fields

    private VehicleOption<VehicleType>? _selectedVehicleType;
    private VehicleOption<VehicleConfiguration>? _selectedLightConfig;
    private VehicleOption<VehicleConfiguration>? _selectedHeavyConfig;
    private VehicleOption<Class4PowerUnitType>? _selectedClass4UnitType;
    private VehicleOption<FuelType>? _selectedFuelType;

    private string _unit1Rego = string.Empty;
    private string _unit2Rego = string.Empty;
    private string _unit3Rego = string.Empty;

    private DateTime? _unit1RegoExpiry;
    private DateTime? _unit2RegoExpiry;
    private DateTime? _unit3RegoExpiry;

    private int? _powerUnitOdometerKm;
    private int? _trailer1OdometerKm;
    private int? _trailer2OdometerKm;

    #endregion

    #region Pickers (Options)

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
            _selectedLightConfig = null;
            _selectedHeavyConfig = null;
            _selectedClass4UnitType = null;
            _selectedFuelType = null;

            // clear regos
            _unit1Rego = string.Empty;
            _unit2Rego = string.Empty;
            _unit3Rego = string.Empty;

            // clear expiries
            _unit1RegoExpiry = null;
            _unit2RegoExpiry = null;
            _unit3RegoExpiry = null;

            // clear odos
            _powerUnitOdometerKm = null;
            _trailer1OdometerKm = null;
            _trailer2OdometerKm = null;

            OnPropertyChanged(nameof(SelectedVehicleType));
            OnPropertyChanged(nameof(SelectedLightConfig));
            OnPropertyChanged(nameof(SelectedHeavyConfig));
            OnPropertyChanged(nameof(SelectedClass4UnitType));
            OnPropertyChanged(nameof(SelectedFuelType));

            OnPropertyChanged(nameof(Unit1Rego));
            OnPropertyChanged(nameof(Unit2Rego));
            OnPropertyChanged(nameof(Unit3Rego));

            OnPropertyChanged(nameof(Unit1RegoExpiry));
            OnPropertyChanged(nameof(Unit2RegoExpiry));
            OnPropertyChanged(nameof(Unit3RegoExpiry));

            OnPropertyChanged(nameof(PowerUnitOdometerKm));
            OnPropertyChanged(nameof(Trailer1OdometerKm));
            OnPropertyChanged(nameof(Trailer2OdometerKm));

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

            // changing unit type should reset dependent flow
            SelectedFuelType = null;
            Unit1RegoExpiry = null;
            Unit2RegoExpiry = null;
            Unit3RegoExpiry = null;

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
        set
        {
            _unit1Rego = value;
            OnPropertyChanged();

            // if user edits rego, expiry/fuel/odo should re-evaluate
            RaiseAllVisibility();
        }
    }

    public string Unit2Rego
    {
        get => _unit2Rego;
        set
        {
            _unit2Rego = value;
            OnPropertyChanged();
            RaiseAllVisibility();
        }
    }

    public string Unit3Rego
    {
        get => _unit3Rego;
        set
        {
            _unit3Rego = value;
            OnPropertyChanged();
            RaiseAllVisibility();
        }
    }

    #endregion

    #region Rego Expiry

    public DateTime? Unit1RegoExpiry
    {
        get => _unit1RegoExpiry;
        set
        {
            _unit1RegoExpiry = value;
            OnPropertyChanged();
            RaiseAllVisibility();
        }
    }

    public DateTime? Unit2RegoExpiry
    {
        get => _unit2RegoExpiry;
        set
        {
            _unit2RegoExpiry = value;
            OnPropertyChanged();
            RaiseAllVisibility();
        }
    }

    public DateTime? Unit3RegoExpiry
    {
        get => _unit3RegoExpiry;
        set
        {
            _unit3RegoExpiry = value;
            OnPropertyChanged();
            RaiseAllVisibility();
        }
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

    // Fuel only AFTER expiry is complete
    public bool ShowFuelType => RegoExpiryComplete;

    // Odo only AFTER fuel chosen
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
