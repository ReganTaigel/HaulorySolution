using System.Collections.ObjectModel;
using Haulory.Domain.Enums;

namespace Haulory.Moblie.ViewModels;

public class VehicleViewModel : BaseViewModel
{
    #region Fields

    private VehicleOption<VehicleType>? _selectedVehicleType;
    private VehicleOption<VehicleConfiguration>? _selectedLightConfig;
    private VehicleOption<VehicleConfiguration>? _selectedHeavyConfig;
    private VehicleOption<FuelType>? _selectedFuelType;

    private string _unit1Rego = string.Empty;
    private string _unit2Rego = string.Empty;
    private string _unit3Rego = string.Empty;

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

    public ObservableCollection<VehicleOption<VehicleConfiguration>> HeavyConfigurations { get; } =
        new()
        {
            new(VehicleConfiguration.Rigid, "Rigid"),
            new(VehicleConfiguration.TractorSemi, "Tractor + Semi"),
            new(VehicleConfiguration.TruckAndTrailer, "Rigid + Trailer"),
            new(VehicleConfiguration.BTrain, "B-Train (tractor + 2 trailers)")
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
            SelectedLightConfig = null;
            SelectedHeavyConfig = null;
            SelectedFuelType = null;

            // clear regos when type changes
            Unit1Rego = string.Empty;
            Unit2Rego = string.Empty;
            Unit3Rego = string.Empty;

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

            // changing config affects rego count
            Unit2Rego = string.Empty;
            Unit3Rego = string.Empty;

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

            Unit2Rego = string.Empty;
            Unit3Rego = string.Empty;

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
            OnPropertyChanged();
            OnPropertyChanged(nameof(RequiresRuc));
            OnPropertyChanged(nameof(FuelInfoText));
        }
    }

    #endregion

    #region Rego Fields (Unit 1/2/3)

    // Unit 1 = main vehicle (car/ute/truck/tractor)
    public string Unit1Rego
    {
        get => _unit1Rego;
        set { _unit1Rego = value; OnPropertyChanged(); }
    }

    // Unit 2 = trailer OR semi OR trailer1
    public string Unit2Rego
    {
        get => _unit2Rego;
        set { _unit2Rego = value; OnPropertyChanged(); }
    }

    // Unit 3 = trailer2 (B-train only)
    public string Unit3Rego
    {
        get => _unit3Rego;
        set { _unit3Rego = value; OnPropertyChanged(); }
    }

    #endregion

    #region Flow / Visibility

    public bool HasVehicleType => SelectedVehicleType != null;

    // show trailer config only for car/ute + trailer
    public bool ShowLightTrailerConfig =>
        SelectedVehicleType?.Value == VehicleType.CarWithTrailer ||
        SelectedVehicleType?.Value == VehicleType.UteWithTrailer;

    // show heavy config for class4 / class4+5 (because you want configuration question)
    public bool ShowHeavyConfig =>
        SelectedVehicleType?.Value == VehicleType.TruckClass4 ||
        SelectedVehicleType?.Value == VehicleType.TruckClass4AndTrailerClass5;

    // ready to ask rego once required config has been chosen (if needed)
    public bool ReadyForRego =>
        HasVehicleType &&
        (!ShowLightTrailerConfig || SelectedLightConfig != null) &&
        (!ShowHeavyConfig || SelectedHeavyConfig != null);

    // ask fuel only AFTER rego stage is visible (keeps it one step at a time)
    public bool ShowFuelType => ReadyForRego;

    // rego counts
    public bool ShowUnit1Rego => ReadyForRego;
    public bool ShowUnit2Rego => ReadyForRego && RegoCount >= 2;
    public bool ShowUnit3Rego => ReadyForRego && RegoCount >= 3;

    public int RegoCount
    {
        get
        {
            if (!HasVehicleType) return 0;

            // car/ute without trailer => 1
            if (SelectedVehicleType!.Value == VehicleType.Car ||
                SelectedVehicleType.Value == VehicleType.Ute ||
                SelectedVehicleType.Value == VehicleType.TruckClass2 ||
                SelectedVehicleType.Value == VehicleType.TruckClass4)
            {
                // class 4 may become >1 depending on chosen heavy config
                if (SelectedVehicleType.Value == VehicleType.TruckClass4 &&
                    SelectedHeavyConfig?.Value == VehicleConfiguration.BTrain)
                    return 3;

                if (SelectedVehicleType.Value == VehicleType.TruckClass4 &&
                    (SelectedHeavyConfig?.Value == VehicleConfiguration.TruckAndTrailer ||
                     SelectedHeavyConfig?.Value == VehicleConfiguration.TractorSemi))
                    return 2;

                return 1;
            }

            // explicit trailer types => 2
            if (SelectedVehicleType.Value == VehicleType.CarWithTrailer ||
                SelectedVehicleType.Value == VehicleType.UteWithTrailer ||
                SelectedVehicleType.Value == VehicleType.TruckClass2AndTrailerClass3 ||
                SelectedVehicleType.Value == VehicleType.TruckClass4AndTrailerClass5)
            {
                // if class4+5 chosen and config is BTrain => 3
                if (SelectedVehicleType.Value == VehicleType.TruckClass4AndTrailerClass5 &&
                    SelectedHeavyConfig?.Value == VehicleConfiguration.BTrain)
                    return 3;

                return 2;
            }

            return 1;
        }
    }

    public string Unit1Label =>
        RegoCount >= 1 ? "Vehicle rego" : string.Empty;

    public string Unit2Label
    {
        get
        {
            if (RegoCount < 2) return string.Empty;

            // tractor+semi wording
            if (SelectedHeavyConfig?.Value == VehicleConfiguration.TractorSemi)
                return "Semi rego";

            return "Trailer rego";
        }
    }

    public string Unit3Label =>
        RegoCount >= 3 ? "Trailer 2 rego" : string.Empty;

    private void RaiseAllVisibility()
    {
        OnPropertyChanged(nameof(HasVehicleType));
        OnPropertyChanged(nameof(ShowLightTrailerConfig));
        OnPropertyChanged(nameof(ShowHeavyConfig));
        OnPropertyChanged(nameof(ReadyForRego));
        OnPropertyChanged(nameof(ShowFuelType));

        OnPropertyChanged(nameof(RegoCount));
        OnPropertyChanged(nameof(ShowUnit1Rego));
        OnPropertyChanged(nameof(ShowUnit2Rego));
        OnPropertyChanged(nameof(ShowUnit3Rego));

        OnPropertyChanged(nameof(Unit1Label));
        OnPropertyChanged(nameof(Unit2Label));
        OnPropertyChanged(nameof(Unit3Label));
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

