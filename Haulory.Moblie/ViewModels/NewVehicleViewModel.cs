using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;
using Haulory.Domain.Enums;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Haulory.Mobile.ViewModels;

public class NewVehicleViewModel : BaseViewModel
{
    #region Fields

    private readonly IVehicleAssetRepository _vehicleRepository;
    private bool _isSaving;

    // Slot conventions (important for future trailer swapping / recall)
    private const int POWER_UNIT_SLOT = 1;   // powered vehicles ALWAYS use Unit 1
    private const int TRAILER_1_SLOT = 2;   // single trailer ALWAYS uses Unit 2
    private const int TRAILER_2_SLOT = 3;   // B-Train second trailer uses Unit 3

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

    // RUC licence range (PER UNIT SLOT)
    private int? _unit1RucLicenceStartKm;
    private int? _unit1RucLicenceEndKm;

    private int? _unit2RucLicenceStartKm;
    private int? _unit2RucLicenceEndKm;

    private int? _unit3RucLicenceStartKm;
    private int? _unit3RucLicenceEndKm;


    // RUC (PER UNIT SLOT)
    private DateTime? _unit1RucPurchasedDate;
    private int? _unit1RucDistancePurchasedKm;

    private DateTime? _unit2RucPurchasedDate;
    private int? _unit2RucDistancePurchasedKm;

    private DateTime? _unit3RucPurchasedDate;
    private int? _unit3RucDistancePurchasedKm;

    // COF/WOF Expiry (per asset)
    private DateTime? _unit1CertExpiry;
    private DateTime? _unit2CertExpiry;
    private DateTime? _unit3CertExpiry;

    // Make, Model, Year
    private string _unit1Make = string.Empty;
    private string _unit1Model = string.Empty;
    private int? _unit1Year;

    private string _unit2Make = string.Empty;
    private string _unit2Model = string.Empty;
    private int? _unit2Year;

    private string _unit3Make = string.Empty;
    private string _unit3Model = string.Empty;
    private int? _unit3Year;

    #endregion

    #region Commands

    public ICommand SaveVehicleCommand { get; }

    public bool CanSaveVehicle
    {
        get
        {
            if (_isSaving) return false;
            if (SelectedVehicleType == null) return false;

            // Must finish each step in order
            if (!ReadyForRego) return false;
            if (!RegoComplete) return false;
            if (!RegoExpiryComplete) return false;
            if (!MakeModelComplete) return false;
            if (!CertificateComplete) return false;

            // Fuel required only for powered vehicles
            if (IsPoweredVehicle && SelectedFuelType == null) return false;

            // Odo required only when OdoCount > 0, and only after odo step is visible
            if (OdoCount > 0)
            {
                if (!ShowOdometers) return false;

                // Powered vehicles: Unit1 odo
                if (IsPoweredVehicle && PowerUnitOdometerKm == null) return false;

                // Heavy trailers: Unit2 odo, and Unit3 odo if B-train
                if (IsHeavyTrailer && Trailer1OdometerKm == null) return false;
                if (IsBTrain && Trailer2OdometerKm == null) return false;
            }

            // RUC must be complete if it applies
            if (!RucComplete) return false;

            return true;
        }
    }

    public bool ShowSaveButton =>
        // If no RUC step, show at the end of odo/cert (same as before)
        (!ShowRucStep && ((OdoCount == 0 && CertificateComplete) || (OdoCount > 0 && ShowOdometers)))
        // If RUC step applies, only show after it’s complete
        || (ShowRucStep && RucComplete);

    private async Task ExecuteSaveVehicleAsync()
    {
        if (!CanSaveVehicle) return;

        try
        {
            _isSaving = true;
            RefreshSaveState();

            await SaveVehicleAsync();

            await Shell.Current.DisplayAlertAsync("Saved", "Vehicle saved successfully.", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Save failed", ex.Message, "OK");
        }
        finally
        {
            _isSaving = false;
            RefreshSaveState();
        }
    }

    private void RefreshSaveState()
    {
        OnPropertyChanged(nameof(CanSaveVehicle));
        OnPropertyChanged(nameof(ShowSaveButton));
        (SaveVehicleCommand as Command)?.ChangeCanExecute();
    }

    #endregion

    #region Constructor

    public NewVehicleViewModel(IVehicleAssetRepository vehicleRepository)
    {
        _vehicleRepository = vehicleRepository;

        SaveVehicleCommand = new Command(
            async () => await ExecuteSaveVehicleAsync(),
            () => CanSaveVehicle);

        RefreshSaveState();
    }

    #endregion

    #region Pickers (Options)

    public ObservableCollection<VehicleOption<VehicleType>> VehicleTypes { get; } =
        new()
        {
            new(VehicleType.Car, "Car"),
            new(VehicleType.Ute, "Ute"),

            // Trailer as its own asset
            new(VehicleType.LightVehicleTrailer, "Light Vehicle Trailer"),

            new(VehicleType.TruckClass2, "Truck (Class 2)"),
            new(VehicleType.TrailerClass3, "Trailer (Class 3)"),
            new(VehicleType.TruckClass4, "Truck (Class 4)"),
            new(VehicleType.TrailerClass5, "Trailer (Class 5)")
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
            new(VehicleConfiguration.SemiFlatDeck, "Semi Flat Trailer"),
            new(VehicleConfiguration.SemiRefrigerator, "Semi Refrigerator Trailer"),
            new(VehicleConfiguration.SemiSkeleton, "Semi Skeleton Trailer"),
            new(VehicleConfiguration.CurtainSiderTrailer, "Single Curtainsider Trailer"),
            new(VehicleConfiguration.BTrainCurtainSider, "B-Train (2 curtain trailers)")
        };

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

            SelectedLightConfig = null;
            SelectedHeavyConfig = null;
            SelectedClass4UnitType = null;
            SelectedFuelType = null;

            // Clear all fields (we’ll use only the correct slots depending on selection)
            Unit1Rego = Unit2Rego = Unit3Rego = string.Empty;
            Unit1RegoExpiry = Unit2RegoExpiry = Unit3RegoExpiry = null;

            Unit1Make = Unit1Model = string.Empty; Unit1Year = null;
            Unit2Make = Unit2Model = string.Empty; Unit2Year = null;
            Unit3Make = Unit3Model = string.Empty; Unit3Year = null;

            Unit1CertExpiry = Unit2CertExpiry = Unit3CertExpiry = null;

            ResetOdometers();
            ResetAllRuc();

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

            // If you switch away from B-Train, clear Unit3 details (including RUC)
            if (!IsBTrain)
            {
                Unit3Rego = string.Empty;
                Unit3RegoExpiry = null;
                Unit3Make = string.Empty;
                Unit3Model = string.Empty;
                Unit3Year = null;
                Unit3CertExpiry = null;
                Trailer2OdometerKm = null;

                ResetUnit3Ruc();
            }

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

            // If powered vehicle fuel does NOT require RUC, clear ONLY Unit1 RUC.
            // Trailer RUC is independent of fuel.
            if (!RequiresRuc)
                ResetUnit1Ruc();

            OnPropertyChanged();
            OnPropertyChanged(nameof(RequiresRuc));
            OnPropertyChanged(nameof(FuelInfoText));
            RaiseAllVisibility();
        }
    }

    private void ResetOdometers()
    {
        PowerUnitOdometerKm = null;
        Trailer1OdometerKm = null;
        Trailer2OdometerKm = null;

        RefreshSaveState();
    }

    #endregion

    #region Rego Fields

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

    #region Make, Model, Year

    public string Unit1Make
    {
        get => _unit1Make;
        set { _unit1Make = value; OnPropertyChanged(); RaiseAllVisibility(); }
    }

    public string Unit1Model
    {
        get => _unit1Model;
        set { _unit1Model = value; OnPropertyChanged(); RaiseAllVisibility(); }
    }

    public int? Unit1Year
    {
        get => _unit1Year;
        set { _unit1Year = value; OnPropertyChanged(); RaiseAllVisibility(); }
    }

    public string Unit2Make
    {
        get => _unit2Make;
        set { _unit2Make = value; OnPropertyChanged(); RaiseAllVisibility(); }
    }

    public string Unit2Model
    {
        get => _unit2Model;
        set { _unit2Model = value; OnPropertyChanged(); RaiseAllVisibility(); }
    }

    public int? Unit2Year
    {
        get => _unit2Year;
        set { _unit2Year = value; OnPropertyChanged(); RaiseAllVisibility(); }
    }

    public string Unit3Make
    {
        get => _unit3Make;
        set { _unit3Make = value; OnPropertyChanged(); RaiseAllVisibility(); }
    }

    public string Unit3Model
    {
        get => _unit3Model;
        set { _unit3Model = value; OnPropertyChanged(); RaiseAllVisibility(); }
    }

    public int? Unit3Year
    {
        get => _unit3Year;
        set { _unit3Year = value; OnPropertyChanged(); RaiseAllVisibility(); }
    }

    public bool ShowMakeModel => RegoExpiryComplete;

    public bool MakeModelComplete
    {
        get
        {
            if (!ShowMakeModel) return false;

            if (IsPoweredVehicle)
            {
                return
                    !string.IsNullOrWhiteSpace(Unit1Make) &&
                    !string.IsNullOrWhiteSpace(Unit1Model) &&
                    Unit1Year != null;
            }

            if (string.IsNullOrWhiteSpace(Unit2Make) ||
                string.IsNullOrWhiteSpace(Unit2Model) ||
                Unit2Year == null)
                return false;

            if (IsBTrain)
            {
                if (string.IsNullOrWhiteSpace(Unit3Make) ||
                    string.IsNullOrWhiteSpace(Unit3Model) ||
                    Unit3Year == null)
                    return false;
            }

            return true;
        }
    }

    public string Unit1MakeModelLabel => "Vehicle make, model & year";
    public string Unit2MakeModelLabel => "Trailer make, model & year";
    public string Unit3MakeModelLabel => "Trailer 2 make, model & year";

    #endregion

    #region Certificate Expiry

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

    #region Odometers

    public int? PowerUnitOdometerKm
    {
        get => _powerUnitOdometerKm;
        set { _powerUnitOdometerKm = value; OnPropertyChanged(); RefreshSaveState(); }
    }

    public int? Trailer1OdometerKm
    {
        get => _trailer1OdometerKm;
        set { _trailer1OdometerKm = value; OnPropertyChanged(); RefreshSaveState(); }
    }

    public int? Trailer2OdometerKm
    {
        get => _trailer2OdometerKm;
        set { _trailer2OdometerKm = value; OnPropertyChanged(); RefreshSaveState(); }
    }

    #endregion

    #region RUC (Road User Charges) - SLOT BASED

    // RUC applies to:
    // - Powered vehicles only when fuel RequiresRuc (Diesel/Electric)
    // - Heavy trailers always (Class 3 + Class 5)

    public bool RequiresRuc =>
        SelectedFuelType?.Value == FuelType.Diesel ||
        SelectedFuelType?.Value == FuelType.Electric;

    public bool TrailerRequiresRuc =>
        SelectedVehicleType?.Value == VehicleType.TrailerClass3 ||
        SelectedVehicleType?.Value == VehicleType.TrailerClass5;

    // How many RUC entries this wizard needs (mirrors unit slot rules)
    public int RucCount
    {
        get
        {
            if (!HasVehicleType) return 0;

            // Powered vehicles: only if fuel requires it
            if (IsPoweredVehicle)
                return (SelectedFuelType != null && RequiresRuc) ? 1 : 0;

            // Heavy trailers: always require it, per trailer unit (Unit2 + Unit3 if B-train)
            if (IsHeavyTrailer)
                return IsBTrain ? 2 : 1;

            // Light trailers: none
            return 0;
        }
    }

    public bool ShowRucStep => ShowOdometers && RucCount > 0;

    // Per-slot visibility
    public bool ShowUnit1Ruc => ShowRucStep && IsPoweredVehicle && RucCount == 1;
    public bool ShowUnit2Ruc => ShowRucStep && !IsPoweredVehicle && IsHeavyTrailer; // trailer-only wizard uses Unit2 (+Unit3)
    public bool ShowUnit3Ruc => ShowRucStep && IsBTrain;

    // Labels
    public string Unit1RucLabel => "Vehicle RUC";
    public string Unit2RucLabel => "Trailer RUC";
    public string Unit3RucLabel => "Trailer 2 RUC";

    private bool IsValidRange(int? start, int? end) =>
        start != null && end != null && start >= 0 && end > start;

    private bool Unit1RucComplete =>
        !ShowUnit1Ruc || IsValidRange(Unit1RucLicenceStartKm, Unit1RucLicenceEndKm);

    private bool Unit2RucComplete =>
        !ShowUnit2Ruc || IsValidRange(Unit2RucLicenceStartKm, Unit2RucLicenceEndKm);

    private bool Unit3RucComplete =>
        !ShowUnit3Ruc || IsValidRange(Unit3RucLicenceStartKm, Unit3RucLicenceEndKm);

    public bool RucComplete => Unit1RucComplete && Unit2RucComplete && Unit3RucComplete;


    // Unit 1 RUC
    public DateTime? Unit1RucPurchasedDate
    {
        get => _unit1RucPurchasedDate;
        set { _unit1RucPurchasedDate = value; OnPropertyChanged(); RefreshSaveState(); }
    }

    public int? Unit1RucDistancePurchasedKm
    {
        get => _unit1RucDistancePurchasedKm;
        set { _unit1RucDistancePurchasedKm = value; OnPropertyChanged(); RefreshSaveState(); }
    }

    // Unit 1 licence range
    public int? Unit1RucLicenceStartKm
    {
        get => _unit1RucLicenceStartKm;
        set { _unit1RucLicenceStartKm = value; OnPropertyChanged(); RefreshSaveState(); }
    }
    public int? Unit1RucLicenceEndKm
    {
        get => _unit1RucLicenceEndKm;
        set { _unit1RucLicenceEndKm = value; OnPropertyChanged(); RefreshSaveState(); }
    }

    // Unit 2 RUC
    public DateTime? Unit2RucPurchasedDate
    {
        get => _unit2RucPurchasedDate;
        set { _unit2RucPurchasedDate = value; OnPropertyChanged(); RefreshSaveState(); }
    }

    public int? Unit2RucDistancePurchasedKm
    {
        get => _unit2RucDistancePurchasedKm;
        set { _unit2RucDistancePurchasedKm = value; OnPropertyChanged(); RefreshSaveState(); }
    }

    // Unit 2 licence range
    public int? Unit2RucLicenceStartKm
    {
        get => _unit2RucLicenceStartKm;
        set { _unit2RucLicenceStartKm = value; OnPropertyChanged(); RefreshSaveState(); }
    }
    public int? Unit2RucLicenceEndKm
    {
        get => _unit2RucLicenceEndKm;
        set { _unit2RucLicenceEndKm = value; OnPropertyChanged(); RefreshSaveState(); }
    }

    // Unit 3 RUC
    public DateTime? Unit3RucPurchasedDate
    {
        get => _unit3RucPurchasedDate;
        set { _unit3RucPurchasedDate = value; OnPropertyChanged(); RefreshSaveState(); }
    }

    public int? Unit3RucDistancePurchasedKm
    {
        get => _unit3RucDistancePurchasedKm;
        set { _unit3RucDistancePurchasedKm = value; OnPropertyChanged(); RefreshSaveState(); }
    }

    // Unit 3 licence range
    public int? Unit3RucLicenceStartKm
    {
        get => _unit3RucLicenceStartKm;
        set { _unit3RucLicenceStartKm = value; OnPropertyChanged(); RefreshSaveState(); }
    }
    public int? Unit3RucLicenceEndKm
    {
        get => _unit3RucLicenceEndKm;
        set { _unit3RucLicenceEndKm = value; OnPropertyChanged(); RefreshSaveState(); }
    }

    private void ResetUnit1Ruc()
    {
        Unit1RucPurchasedDate = null;
        Unit1RucDistancePurchasedKm = null;
        Unit1RucLicenceStartKm = null;
        Unit1RucLicenceEndKm = null;
    }

    private void ResetUnit2Ruc()
    {
        Unit2RucPurchasedDate = null;
        Unit2RucDistancePurchasedKm = null;
        Unit2RucLicenceStartKm = null;
        Unit2RucLicenceEndKm = null;
    }

    private void ResetUnit3Ruc()
    {
        Unit3RucPurchasedDate = null;
        Unit3RucDistancePurchasedKm = null;
        Unit3RucLicenceStartKm = null;
        Unit3RucLicenceEndKm = null;
    }

    private void ResetAllRuc()
    {
        ResetUnit1Ruc();
        ResetUnit2Ruc();
        ResetUnit3Ruc();
    }

    public string FuelInfoText =>
        SelectedFuelType == null
            ? string.Empty
            : RequiresRuc
                ? "Road User Charges (RUC) apply to this vehicle."
                : "No Road User Charges (RUC) required for this fuel type.";

    #endregion

    #region Flow / Visibility (UPDATED with Unit Slot Rules)

    public bool HasVehicleType => SelectedVehicleType != null;

    // powered assets (need fuel + odo)
    public bool IsPoweredVehicle =>
        SelectedVehicleType?.Value == VehicleType.Car ||
        SelectedVehicleType?.Value == VehicleType.Ute ||
        SelectedVehicleType?.Value == VehicleType.TruckClass2 ||
        SelectedVehicleType?.Value == VehicleType.TruckClass4;

    public bool IsTrailerAsset =>
        SelectedVehicleType?.Value == VehicleType.LightVehicleTrailer ||
        SelectedVehicleType?.Value == VehicleType.TrailerClass3 ||
        SelectedVehicleType?.Value == VehicleType.TrailerClass5;

    public bool IsLightTrailer => SelectedVehicleType?.Value == VehicleType.LightVehicleTrailer;

    public bool IsHeavyTrailer =>
        SelectedVehicleType?.Value == VehicleType.TrailerClass3 ||
        SelectedVehicleType?.Value == VehicleType.TrailerClass5;

    public bool IsBTrain =>
        SelectedVehicleType?.Value == VehicleType.TrailerClass5 &&
        SelectedHeavyConfig?.Value == VehicleConfiguration.BTrainCurtainSider;

    public bool ShowLightTrailerConfig => SelectedVehicleType?.Value == VehicleType.LightVehicleTrailer;

    public bool ShowClass4UnitType => SelectedVehicleType?.Value == VehicleType.TruckClass4;

    public bool ShowHeavyConfig => SelectedVehicleType?.Value == VehicleType.TrailerClass5;

    public bool ReadyForRego =>
        HasVehicleType &&
        (!ShowLightTrailerConfig || SelectedLightConfig != null) &&
        (!ShowClass4UnitType || SelectedClass4UnitType != null) &&
        (!ShowHeavyConfig || SelectedHeavyConfig != null);

    public bool IsHeavyVehicle =>
        SelectedVehicleType?.Value == VehicleType.TruckClass2 ||
        SelectedVehicleType?.Value == VehicleType.TruckClass4 ||
        SelectedVehicleType?.Value == VehicleType.TrailerClass3 ||
        SelectedVehicleType?.Value == VehicleType.TrailerClass5;

    public ComplianceCertificateType RequiredCertificate =>
        IsHeavyVehicle ? ComplianceCertificateType.Cof : ComplianceCertificateType.Wof;

    public string CertificateName => RequiredCertificate == ComplianceCertificateType.Cof ? "COF" : "WOF";
    public string CertificatePluralName => RequiredCertificate == ComplianceCertificateType.Cof ? "COFs" : "WOFs";

    // How many regos/certs this screen is collecting (by slot rule)
    public int RegoCount
    {
        get
        {
            if (!HasVehicleType) return 0;

            if (IsPoweredVehicle) return 1; // Unit1
            if (IsBTrain) return 2;         // Unit2 + Unit3
            return 1;                       // Unit2 only
        }
    }

    // ODO RULES by slot rule:
    // - Powered vehicles: 1 odo (Unit1)
    // - Light trailers: 0
    // - Heavy trailers: 1 odo per trailer => Unit2 (and Unit3 if B-train)
    public int OdoCount
    {
        get
        {
            if (!HasVehicleType) return 0;

            if (IsPoweredVehicle) return 1;
            if (IsLightTrailer) return 0;
            if (IsHeavyTrailer) return IsBTrain ? 2 : 1;

            return 0;
        }
    }

    public bool RegoComplete
    {
        get
        {
            if (!ReadyForRego) return false;

            if (IsPoweredVehicle)
                return !string.IsNullOrWhiteSpace(Unit1Rego);

            if (string.IsNullOrWhiteSpace(Unit2Rego)) return false;
            if (IsBTrain && string.IsNullOrWhiteSpace(Unit3Rego)) return false;

            return true;
        }
    }

    public bool ShowRegoExpiry => ReadyForRego && RegoComplete;

    public bool RegoExpiryComplete
    {
        get
        {
            if (!ShowRegoExpiry) return false;

            if (IsPoweredVehicle)
                return Unit1RegoExpiry != null;

            if (Unit2RegoExpiry == null) return false;
            if (IsBTrain && Unit3RegoExpiry == null) return false;

            return true;
        }
    }

    public bool ShowCertificateStep => MakeModelComplete;

    public int CertificateCount => RegoCount;

    public bool ShowUnit1Cert => ShowCertificateStep && IsPoweredVehicle;
    public bool ShowUnit2Cert => ShowCertificateStep && IsTrailerAsset;
    public bool ShowUnit3Cert => ShowCertificateStep && IsBTrain;

    public string Unit1Label => "Vehicle rego";
    public string Unit2Label => "Trailer rego";
    public string Unit3Label => "Trailer 2 rego";

    public string Unit1CertLabel => $"{Unit1Label} {CertificateName} expiry";
    public string Unit2CertLabel => $"{Unit2Label} {CertificateName} expiry";
    public string Unit3CertLabel => $"{Unit3Label} {CertificateName} expiry";

    public bool CertificateComplete
    {
        get
        {
            if (!ShowCertificateStep) return false;

            if (IsPoweredVehicle)
                return Unit1CertExpiry != null;

            if (Unit2CertExpiry == null) return false;
            if (IsBTrain && Unit3CertExpiry == null) return false;

            return true;
        }
    }

    // Fuel only for powered vehicles
    public bool ShowFuelType => CertificateComplete && IsPoweredVehicle;

    // Odometers:
    // - Powered vehicles: after fuel
    // - Heavy trailers: after certificate (no fuel step)
    public bool ShowOdometers =>
        CertificateComplete &&
        (
            (IsPoweredVehicle && SelectedFuelType != null) ||
            (IsHeavyTrailer)
        );

    // Rego slot visibility
    public bool ShowUnit1Rego => ReadyForRego && IsPoweredVehicle;
    public bool ShowUnit2Rego => ReadyForRego && IsTrailerAsset;
    public bool ShowUnit3Rego => ReadyForRego && IsBTrain;

    // Odo slot visibility
    public bool ShowPowerUnitOdo => ShowOdometers && IsPoweredVehicle;
    public bool ShowTrailer1Odo => ShowOdometers && IsHeavyTrailer;
    public bool ShowTrailer2Odo => ShowOdometers && IsBTrain;

    public string PowerUnitOdoLabel => "Odometer (km)";
    public string Trailer1OdoLabel => "Trailer odometer (km)";
    public string Trailer2OdoLabel => "Trailer 2 odometer (km)";

    private void RaiseAllVisibility()
    {
        // Core selection + gating
        OnPropertyChanged(nameof(HasVehicleType));
        OnPropertyChanged(nameof(IsPoweredVehicle));
        OnPropertyChanged(nameof(IsTrailerAsset));
        OnPropertyChanged(nameof(IsLightTrailer));
        OnPropertyChanged(nameof(IsHeavyTrailer));
        OnPropertyChanged(nameof(IsBTrain));

        // Pickers / next-step sections
        OnPropertyChanged(nameof(ShowLightTrailerConfig));
        OnPropertyChanged(nameof(ShowClass4UnitType));
        OnPropertyChanged(nameof(ShowHeavyConfig));
        OnPropertyChanged(nameof(ReadyForRego));

        // Counts that drive how many fields appear
        OnPropertyChanged(nameof(RegoCount));
        OnPropertyChanged(nameof(OdoCount));

        // Rego slot visibility + labels
        OnPropertyChanged(nameof(ShowUnit1Rego));
        OnPropertyChanged(nameof(ShowUnit2Rego));
        OnPropertyChanged(nameof(ShowUnit3Rego));
        OnPropertyChanged(nameof(Unit1Label));
        OnPropertyChanged(nameof(Unit2Label));
        OnPropertyChanged(nameof(Unit3Label));

        // Rego flow
        OnPropertyChanged(nameof(RegoComplete));
        OnPropertyChanged(nameof(ShowRegoExpiry));
        OnPropertyChanged(nameof(RegoExpiryComplete));

        // Make/Model flow
        OnPropertyChanged(nameof(ShowMakeModel));
        OnPropertyChanged(nameof(MakeModelComplete));
        OnPropertyChanged(nameof(Unit1MakeModelLabel));
        OnPropertyChanged(nameof(Unit2MakeModelLabel));
        OnPropertyChanged(nameof(Unit3MakeModelLabel));

        // Certificate flow
        OnPropertyChanged(nameof(IsHeavyVehicle));
        OnPropertyChanged(nameof(RequiredCertificate));
        OnPropertyChanged(nameof(CertificateName));
        OnPropertyChanged(nameof(CertificatePluralName));

        OnPropertyChanged(nameof(ShowCertificateStep));
        OnPropertyChanged(nameof(CertificateCount));
        OnPropertyChanged(nameof(ShowUnit1Cert));
        OnPropertyChanged(nameof(ShowUnit2Cert));
        OnPropertyChanged(nameof(ShowUnit3Cert));
        OnPropertyChanged(nameof(Unit1CertLabel));
        OnPropertyChanged(nameof(Unit2CertLabel));
        OnPropertyChanged(nameof(Unit3CertLabel));
        OnPropertyChanged(nameof(CertificateComplete));

        // Fuel + Odo flow
        OnPropertyChanged(nameof(ShowFuelType));
        OnPropertyChanged(nameof(ShowOdometers));

        OnPropertyChanged(nameof(ShowPowerUnitOdo));
        OnPropertyChanged(nameof(ShowTrailer1Odo));
        OnPropertyChanged(nameof(ShowTrailer2Odo));
        OnPropertyChanged(nameof(PowerUnitOdoLabel));
        OnPropertyChanged(nameof(Trailer1OdoLabel));
        OnPropertyChanged(nameof(Trailer2OdoLabel));

        // Fuel info
        OnPropertyChanged(nameof(RequiresRuc));
        OnPropertyChanged(nameof(TrailerRequiresRuc));
        OnPropertyChanged(nameof(FuelInfoText));

        // RUC flow
        OnPropertyChanged(nameof(RucCount));
        OnPropertyChanged(nameof(ShowRucStep));
        OnPropertyChanged(nameof(ShowUnit1Ruc));
        OnPropertyChanged(nameof(ShowUnit2Ruc));
        OnPropertyChanged(nameof(ShowUnit3Ruc));
        OnPropertyChanged(nameof(Unit1RucLabel));
        OnPropertyChanged(nameof(Unit2RucLabel));
        OnPropertyChanged(nameof(Unit3RucLabel));
        OnPropertyChanged(nameof(RucComplete));

        RefreshSaveState();
    }

    #endregion

    #region Save (ASSETS) - UPDATED to enforce Unit Slot Rules + SLOT RUC (LICENCE RANGE)

    public async Task SaveVehicleAsync()
    {
        if (SelectedVehicleType == null)
            throw new InvalidOperationException("Vehicle type is required.");

        var setId = Guid.NewGuid();
        var certType = RequiredCertificate;

        var assets = new List<VehicleAsset>();

        if (IsPoweredVehicle)
        {
            if (SelectedFuelType == null)
                throw new InvalidOperationException("Fuel type is required for powered vehicles.");

            if (Unit1Year == null)
                throw new InvalidOperationException("Year is required.");

            var odo = PowerUnitOdometerKm;

            var asset = new VehicleAsset
            {
                VehicleSetId = setId,
                UnitNumber = POWER_UNIT_SLOT,
                Kind = AssetKind.PowerUnit,
                VehicleType = SelectedVehicleType.Value,
                FuelType = SelectedFuelType.Value,

                Year = Unit1Year.Value,
                Rego = Unit1Rego.Trim(),
                RegoExpiry = Unit1RegoExpiry,

                Make = Unit1Make.Trim(),
                Model = Unit1Model.Trim(),

                CertificateType = certType,
                CertificateExpiry = Unit1CertExpiry,

                OdometerKm = odo
            };

            // Slot-based RUC (Unit 1 only) when it applies
            if (RucCount == 1)
            {
                // Keep these for stats / cost per km later
                asset.RucPurchasedDate = Unit1RucPurchasedDate;
                asset.RucDistancePurchasedKm = Unit1RucDistancePurchasedKm;

                // NEW: licence range is the compliance source of truth
                asset.RucLicenceStartKm = Unit1RucLicenceStartKm;
                asset.RucLicenceEndKm = Unit1RucLicenceEndKm;

                // Do NOT assume "odo at purchase" == "licence start"
                asset.RucOdometerAtPurchaseKm = null;

                // Next due is derived from licence end (not odo + distance)
                asset.RucNextDueOdometerKm = Unit1RucLicenceEndKm;
            }

            assets.Add(asset);

            await _vehicleRepository.AddRangeAsync(assets);
            return;
        }

        // -------- Trailer-only path --------

        if (Unit2Year == null)
            throw new InvalidOperationException("Trailer year is required.");

        // Trailer #1 ALWAYS Unit 2
        var trailer1Odo = IsHeavyTrailer ? Trailer1OdometerKm : null;

        var trailer1 = new VehicleAsset
        {
            VehicleSetId = setId,
            UnitNumber = TRAILER_1_SLOT,
            Kind = AssetKind.Trailer,
            VehicleType = SelectedVehicleType.Value,

            Configuration =
                SelectedVehicleType.Value == VehicleType.TrailerClass5
                    ? SelectedHeavyConfig?.Value
                    : SelectedVehicleType.Value == VehicleType.LightVehicleTrailer
                        ? SelectedLightConfig?.Value
                        : null,

            Year = Unit2Year.Value,
            Rego = Unit2Rego.Trim(),
            RegoExpiry = Unit2RegoExpiry,

            Make = Unit2Make.Trim(),
            Model = Unit2Model.Trim(),

            CertificateType = certType,
            CertificateExpiry = Unit2CertExpiry,

            OdometerKm = trailer1Odo
        };

        // RUC for heavy trailers (Unit 2)
        if (IsHeavyTrailer)
        {
            // Keep these for stats / cost per km later
            trailer1.RucPurchasedDate = Unit2RucPurchasedDate;
            trailer1.RucDistancePurchasedKm = Unit2RucDistancePurchasedKm;

            // NEW: licence range is the compliance source of truth
            trailer1.RucLicenceStartKm = Unit2RucLicenceStartKm;
            trailer1.RucLicenceEndKm = Unit2RucLicenceEndKm;

            // Do NOT assume "odo at purchase" == "licence start"
            trailer1.RucOdometerAtPurchaseKm = null;

            // Next due is derived from licence end (not odo + distance)
            trailer1.RucNextDueOdometerKm = Unit2RucLicenceEndKm;
        }

        assets.Add(trailer1);

        if (IsBTrain)
        {
            if (Unit3Year == null)
                throw new InvalidOperationException("Trailer 2 year is required.");

            var trailer2Odo = Trailer2OdometerKm;

            var trailer2 = new VehicleAsset
            {
                VehicleSetId = setId,
                UnitNumber = TRAILER_2_SLOT,
                Kind = AssetKind.Trailer,
                VehicleType = SelectedVehicleType.Value,

                Configuration = SelectedHeavyConfig?.Value,

                Year = Unit3Year.Value,
                Rego = Unit3Rego.Trim(),
                RegoExpiry = Unit3RegoExpiry,

                Make = Unit3Make.Trim(),
                Model = Unit3Model.Trim(),

                CertificateType = certType,
                CertificateExpiry = Unit3CertExpiry,

                OdometerKm = trailer2Odo
            };

            // RUC for heavy trailers (Unit 3)
            // (B-train implies heavy trailer config here)
            trailer2.RucPurchasedDate = Unit3RucPurchasedDate;
            trailer2.RucDistancePurchasedKm = Unit3RucDistancePurchasedKm;

            trailer2.RucLicenceStartKm = Unit3RucLicenceStartKm;
            trailer2.RucLicenceEndKm = Unit3RucLicenceEndKm;

            trailer2.RucOdometerAtPurchaseKm = null;
            trailer2.RucNextDueOdometerKm = Unit3RucLicenceEndKm;

            assets.Add(trailer2);
        }

        await _vehicleRepository.AddRangeAsync(assets);
    }

    #endregion

}
