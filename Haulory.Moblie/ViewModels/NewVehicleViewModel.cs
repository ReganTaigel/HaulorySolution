
using Haulory.Domain.Enums;
using Haulory.Mobile.Contracts.Vehicles;
using Haulory.Mobile.Features;
using Haulory.Mobile.Services;
using Haulory.Mobile.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;


namespace Haulory.Mobile.ViewModels;

// Wizard-style ViewModel for creating a "vehicle set" (either a powered vehicle OR trailer-only set).
// Enforces unit-slot conventions:
// - Unit 1: Powered vehicle (Power Unit)
// - Unit 2: Trailer 1 (single trailer uses Unit 2)
// - Unit 3: Trailer 2 (B-Train second trailer uses Unit 3)

public class NewVehicleViewModel : BaseViewModel
{
    #region Dependencies

    private readonly VehiclesApiService _vehiclesApiService;
    #endregion

    #region State Flags

    private bool _isSaving;

    #endregion

    #region Unit Slot Constants

    private const int POWER_UNIT_SLOT = 1;
    private const int TRAILER_1_SLOT = 2;
    private const int TRAILER_2_SLOT = 3;

    #endregion

    #region Backing Fields - Picker Selections

    private VehicleOption<VehicleType>? _selectedVehicleType;
    private VehicleOption<VehicleConfiguration>? _selectedLightConfig;
    private VehicleOption<VehicleConfiguration>? _selectedHeavyConfig;
    private VehicleOption<PowerUnitBodyType>? _selectedPowerUnitBodyType;
    private VehicleOption<FuelType>? _selectedFuelType;

    #endregion

    #region Backing Fields - Rego

    private string _unit1Rego = string.Empty;
    private string _unit2Rego = string.Empty;
    private string _unit3Rego = string.Empty;

    #endregion

    #region Backing Fields - Rego Expiry

    private DateTime? _unit1RegoExpiry;
    private DateTime? _unit2RegoExpiry;
    private DateTime? _unit3RegoExpiry;

    #endregion

    #region Backing Fields - Odometers

    private int? _powerUnitOdometerKm;
    private int? _trailer1OdometerKm;
    private int? _trailer2OdometerKm;

    #endregion

    #region Backing Fields - RUC Licence Range (Per Slot)

    private int? _unit1RucLicenceStartKm;
    private int? _unit1RucLicenceEndKm;

    private int? _unit2RucLicenceStartKm;
    private int? _unit2RucLicenceEndKm;

    private int? _unit3RucLicenceStartKm;
    private int? _unit3RucLicenceEndKm;

    #endregion

    #region Backing Fields - RUC Purchase (Per Slot)

    private DateTime? _unit1RucPurchasedDate;
    private int? _unit1RucDistancePurchasedKm;

    private DateTime? _unit2RucPurchasedDate;
    private int? _unit2RucDistancePurchasedKm;

    private DateTime? _unit3RucPurchasedDate;
    private int? _unit3RucDistancePurchasedKm;

    #endregion

    #region Backing Fields - COF/WOF Expiry (Per Asset)

    private DateTime? _unit1CertExpiry;
    private DateTime? _unit2CertExpiry;
    private DateTime? _unit3CertExpiry;

    #endregion

    #region Backing Fields - Make, Model, Year (Per Asset)

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

    #region Feature Access

    public bool IsAddVehicleVisible => IsFeatureVisible(AppFeature.AddVehicle);
    public bool IsAddVehicleEnabled => IsFeatureEnabled(AppFeature.AddVehicle);
    public bool CanSaveVehicleAction => IsAddVehicleEnabled && CanSaveVehicle;

    #endregion

    #region Commands

    public ICommand SaveVehicleCommand { get; }

    public bool CanSaveVehicle
    {
        get
        {
            if (_isSaving) return false;
            if (SelectedVehicleType == null) return false;

            if (!ReadyForRego) return false;
            if (!RegoComplete) return false;
            if (!RegoExpiryComplete) return false;
            if (!MakeModelComplete) return false;
            if (!CertificateComplete) return false;

            if (IsPoweredVehicle && SelectedFuelType == null) return false;

            if (OdoCount > 0)
            {
                if (!ShowOdometers) return false;

                if (IsPoweredVehicle && PowerUnitOdometerKm == null) return false;
                if (IsHeavyTrailer && Trailer1OdometerKm == null) return false;
                if (IsBTrain && Trailer2OdometerKm == null) return false;
            }

            if (!RucComplete) return false;

            return true;
        }
    }

    public bool ShowSaveButton =>
        IsAddVehicleVisible &&
        (
            (!ShowRucStep && ((OdoCount == 0 && CertificateComplete) || (OdoCount > 0 && ShowOdometers)))
            || (ShowRucStep && RucComplete)
        );

    private async Task ExecuteSaveVehicleAsync()
    {
        if (!await EnsureFeatureEnabledAsync(AppFeature.AddVehicle))
            return;

        if (!CanSaveVehicle)
            return;

        try
        {
            _isSaving = true;
            RefreshSaveState();

            await SaveVehicleAsync();

            await Shell.Current.DisplayAlertAsync("Saved", "Vehicle saved successfully.", "OK");
            await Shell.Current.GoToAsync(nameof(VehicleCollectionPage));
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
        OnPropertyChanged(nameof(IsAddVehicleVisible));
        OnPropertyChanged(nameof(IsAddVehicleEnabled));
        OnPropertyChanged(nameof(CanSaveVehicleAction));
        (SaveVehicleCommand as Command)?.ChangeCanExecute();
    }

    #endregion

    #region Constructor

    public NewVehicleViewModel(
        VehiclesApiService vehiclesApiService,
        IFeatureAccessService featureAccessService)
        : base(featureAccessService)
    {
        _vehiclesApiService = vehiclesApiService;

        SaveVehicleCommand = new Command(async () => await ExecuteSaveVehicleAsync(), () => CanSaveVehicle);

        RefreshSaveState();
    }

    #endregion

    #region Pickers (Options)

    public ObservableCollection<VehicleOption<VehicleType>> VehicleTypes { get; } =
        new()
        {
            new(VehicleType.LightVehicle, "Light Vehicle"),
            new(VehicleType.LightCommercial, "Light Commercial Vehicle"),
            new(VehicleType.RigidTruckMedium, "Medium Rigid Truck"),
            new(VehicleType.RigidTruckHeavy, "Heavy Rigid Truck"),
            new(VehicleType.TractorUnit, "Tractor Unit"),
            new(VehicleType.TrailerLight, "Light Trailer"),
            new(VehicleType.TrailerHeavy, "Heavy Trailer"),
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
            new(VehicleConfiguration.SemiCurtainsider, "Semi Trailer (Curtains)"),
            new(VehicleConfiguration.SemiFlatDeck, "Semi Trailer (Flat Deck)"),
            new(VehicleConfiguration.SemiRefrigerated, "Semi Trailer (Refrigerated)"),
            new(VehicleConfiguration.SemiTanker, "Semi Trailer (Tanker)"),
            new(VehicleConfiguration.SemiSkeleton, "Semi Trailer (Skeleton)"),

            new(VehicleConfiguration.DrawbarCurtainsider, "Curtainsider Trailer (A-Frame)"),
            new(VehicleConfiguration.DrawbarFlatDeck, "Flat Deck (A-Frame)"),
            new(VehicleConfiguration.DrawbarRefrigerated, "Refrigerated Trailer (A-Frame)"),
            new(VehicleConfiguration.DrawbarTanker, "Tanker Trailer (A-Frame)"),

            new(VehicleConfiguration.BDblCurtainsider, "Curtainsider Trailer (Fifth Wheel)"),
            new(VehicleConfiguration.BDblFlatDeck, "Flat Deck Trailer (Fifth Wheel)"),
            new(VehicleConfiguration.BDblRefrigerated, "Refrigerated Trailers (Fifth Wheel)"),
            new(VehicleConfiguration.BDblTanker, "Tanker Trailer (Fifth Wheel)"),
       };

    public ObservableCollection<VehicleOption<PowerUnitBodyType>> PowerUnitBodyTypes { get; } =
        new()
        {
            new(PowerUnitBodyType.Curtainsider, "Rigid Truck (Curtains)"),
            new(PowerUnitBodyType.FlatDeck, "Rigid Truck (Flat Deck)"),
            new(PowerUnitBodyType.Refrigerated, "Rigid Truck (Refrigerated)"),
            new(PowerUnitBodyType.Tanker, "Rigid Truck (Tanker)"),
            new(PowerUnitBodyType.Tractor, "Tractor"),
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

    #region Selected Values (Wizard Choice Inputs)

    public VehicleOption<VehicleType>? SelectedVehicleType
    {
        get => _selectedVehicleType;
        set
        {
            _selectedVehicleType = value;

            SelectedLightConfig = null;
            SelectedHeavyConfig = null;
            SelectedPowerUnitBodyType = null;
            SelectedFuelType = null;

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

    public VehicleOption<PowerUnitBodyType>? SelectedPowerUnitBodyType
    {
        get => _selectedPowerUnitBodyType;
        set
        {
            _selectedPowerUnitBodyType = value;
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

    public string Unit1MakeModelLabel => "Unit 1 — make, model & year";
    public string Unit2MakeModelLabel => "Unit 2 — make, model & year";
    public string Unit3MakeModelLabel => "Unit 3 — make, model & year";

    #endregion

    #region Certificate Expiry (COF/WOF)

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

    #region RUC

    public bool RequiresRuc =>
        SelectedFuelType?.Value == FuelType.Diesel ||
        SelectedFuelType?.Value == FuelType.Electric;

    public bool TrailerRequiresRuc => IsHeavyTrailer;

    public int RucCount
    {
        get
        {
            if (!HasVehicleType) return 0;

            if (IsPoweredVehicle)
                return (SelectedFuelType != null && RequiresRuc) ? 1 : 0;

            if (IsHeavyTrailer)
                return IsBTrain ? 2 : 1;

            return 0;
        }
    }

    public bool ShowRucStep => ShowOdometers && RucCount > 0;

    public bool ShowUnit1Ruc => ShowRucStep && IsPoweredVehicle && RucCount == 1;
    public bool ShowUnit2Ruc => ShowRucStep && !IsPoweredVehicle && IsHeavyTrailer;
    public bool ShowUnit3Ruc => ShowRucStep && IsBTrain;

    public string Unit1RucLabel => "Unit 1 distance-based road tax";
    public string Unit2RucLabel => "Unit 2 distance-based road tax";
    public string Unit3RucLabel => "Unit 3 distance-based road tax";

    private bool IsValidRange(int? start, int? end) =>
        start != null && end != null && start >= 0 && end > start;

    private bool Unit1RucComplete =>
        !ShowUnit1Ruc || IsValidRange(Unit1RucLicenceStartKm, Unit1RucLicenceEndKm);

    private bool Unit2RucComplete =>
        !ShowUnit2Ruc || IsValidRange(Unit2RucLicenceStartKm, Unit2RucLicenceEndKm);

    private bool Unit3RucComplete =>
        !ShowUnit3Ruc || IsValidRange(Unit3RucLicenceStartKm, Unit3RucLicenceEndKm);

    public bool RucComplete => Unit1RucComplete && Unit2RucComplete && Unit3RucComplete;

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
                ? "Distance-based road tax may apply for this fuel type in some regions."
                : "No distance-based road tax is required for this fuel type in most regions.";

    #endregion

    #region Flow / Visibility

    public bool HasVehicleType => SelectedVehicleType != null;

    public bool IsPoweredVehicle =>
        SelectedVehicleType?.Value == VehicleType.LightVehicle ||
        SelectedVehicleType?.Value == VehicleType.LightCommercial ||
        SelectedVehicleType?.Value == VehicleType.RigidTruckMedium ||
        SelectedVehicleType?.Value == VehicleType.RigidTruckHeavy ||
        SelectedVehicleType?.Value == VehicleType.TractorUnit;

    public bool IsTrailerAsset =>
        SelectedVehicleType?.Value == VehicleType.TrailerLight ||
        SelectedVehicleType?.Value == VehicleType.TrailerHeavy;

    public bool IsLightTrailer => SelectedVehicleType?.Value == VehicleType.TrailerLight;

    public bool IsHeavyTrailer =>
        SelectedVehicleType?.Value == VehicleType.TrailerHeavy;

    public bool IsBTrain =>
        IsHeavyTrailer &&
        SelectedHeavyConfig != null &&
        (
            SelectedHeavyConfig.Value == VehicleConfiguration.BDblCurtainsider ||
            SelectedHeavyConfig.Value == VehicleConfiguration.BDblFlatDeck ||
            SelectedHeavyConfig.Value == VehicleConfiguration.BDblRefrigerated ||
            SelectedHeavyConfig.Value == VehicleConfiguration.BDblTanker
        );

    public bool ShowLightTrailerConfig => IsLightTrailer;

    public bool ShowPowerUnitBodyType =>
        SelectedVehicleType?.Value == VehicleType.RigidTruckMedium ||
        SelectedVehicleType?.Value == VehicleType.RigidTruckHeavy;

    public bool ShowHeavyConfig => IsHeavyTrailer;

    public bool ReadyForRego =>
        HasVehicleType &&
        (!ShowLightTrailerConfig || SelectedLightConfig != null) &&
        (!ShowPowerUnitBodyType || _selectedPowerUnitBodyType != null) &&
        (!ShowHeavyConfig || SelectedHeavyConfig != null);

    public bool IsHeavyVehicle =>
        SelectedVehicleType?.Value == VehicleType.RigidTruckMedium ||
        SelectedVehicleType?.Value == VehicleType.RigidTruckHeavy ||
        SelectedVehicleType?.Value == VehicleType.TrailerHeavy;

    public ComplianceCertificateType RequiredCertificate =>
      IsHeavyVehicle ? ComplianceCertificateType.Cof : ComplianceCertificateType.Wof;

    public string CertificateName => "Inspection";
    public string CertificatePluralName => "Inspection / certification";

    public int RegoCount
    {
        get
        {
            if (!HasVehicleType) return 0;

            if (IsPoweredVehicle) return 1;
            if (IsBTrain) return 2;
            return 1;
        }
    }

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

    public string Unit1Label => "Unit 1 registration";
    public string Unit2Label => "Unit 2 registration";
    public string Unit3Label => "Unit 3 registration";

    public string Unit1CertLabel => "Unit 1 inspection/certification expiry";
    public string Unit2CertLabel => "Unit 2 inspection/certification expiry";
    public string Unit3CertLabel => "Unit 3 inspection/certification expiry";

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

    public bool ShowFuelType => CertificateComplete && IsPoweredVehicle;

    public bool ShowOdometers =>
        CertificateComplete &&
        (
            (IsPoweredVehicle && SelectedFuelType != null) ||
            (IsHeavyTrailer)
        );

    public bool ShowUnit1Rego => ReadyForRego && IsPoweredVehicle;
    public bool ShowUnit2Rego => ReadyForRego && IsTrailerAsset;
    public bool ShowUnit3Rego => ReadyForRego && IsBTrain;

    public bool ShowPowerUnitOdo => ShowOdometers && IsPoweredVehicle;
    public bool ShowTrailer1Odo => ShowOdometers && IsHeavyTrailer;
    public bool ShowTrailer2Odo => ShowOdometers && IsBTrain;

    public string PowerUnitOdoLabel => "Odometer";
    public string Trailer1OdoLabel => "Odometer (unit 2)";
    public string Trailer2OdoLabel => "Odometer (unit 3)";

    private void RaiseAllVisibility()
    {
        OnPropertyChanged(nameof(HasVehicleType));
        OnPropertyChanged(nameof(IsPoweredVehicle));
        OnPropertyChanged(nameof(IsTrailerAsset));
        OnPropertyChanged(nameof(IsLightTrailer));
        OnPropertyChanged(nameof(IsHeavyTrailer));
        OnPropertyChanged(nameof(IsBTrain));

        OnPropertyChanged(nameof(ShowLightTrailerConfig));
        OnPropertyChanged(nameof(ShowPowerUnitBodyType));
        OnPropertyChanged(nameof(ShowHeavyConfig));
        OnPropertyChanged(nameof(ReadyForRego));

        OnPropertyChanged(nameof(RegoCount));
        OnPropertyChanged(nameof(OdoCount));

        OnPropertyChanged(nameof(ShowUnit1Rego));
        OnPropertyChanged(nameof(ShowUnit2Rego));
        OnPropertyChanged(nameof(ShowUnit3Rego));
        OnPropertyChanged(nameof(Unit1Label));
        OnPropertyChanged(nameof(Unit2Label));
        OnPropertyChanged(nameof(Unit3Label));

        OnPropertyChanged(nameof(RegoComplete));
        OnPropertyChanged(nameof(ShowRegoExpiry));
        OnPropertyChanged(nameof(RegoExpiryComplete));

        OnPropertyChanged(nameof(ShowMakeModel));
        OnPropertyChanged(nameof(MakeModelComplete));
        OnPropertyChanged(nameof(Unit1MakeModelLabel));
        OnPropertyChanged(nameof(Unit2MakeModelLabel));
        OnPropertyChanged(nameof(Unit3MakeModelLabel));

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

        OnPropertyChanged(nameof(ShowFuelType));
        OnPropertyChanged(nameof(ShowOdometers));

        OnPropertyChanged(nameof(ShowPowerUnitOdo));
        OnPropertyChanged(nameof(ShowTrailer1Odo));
        OnPropertyChanged(nameof(ShowTrailer2Odo));
        OnPropertyChanged(nameof(PowerUnitOdoLabel));
        OnPropertyChanged(nameof(Trailer1OdoLabel));
        OnPropertyChanged(nameof(Trailer2OdoLabel));

        OnPropertyChanged(nameof(RequiresRuc));
        OnPropertyChanged(nameof(TrailerRequiresRuc));
        OnPropertyChanged(nameof(FuelInfoText));

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

    #region Save (Assets)

    public async Task SaveVehicleAsync()
    {
        var units = new List<CreateVehicleUnitRequest>();

        if (IsPoweredVehicle)
        {
            units.Add(new CreateVehicleUnitRequest
            {
                UnitNumber = 1,
                Kind = "PowerUnit",
                VehicleType = SelectedVehicleType?.Value.ToString() ?? "",

                Rego = Unit1Rego,
                RegoExpiry = Unit1RegoExpiry,

                Make = Unit1Make,
                Model = Unit1Model,
                Year = Unit1Year ?? 0,

                FuelType = SelectedFuelType?.Value.ToString(),
                Configuration = SelectedPowerUnitBodyType?.Value.ToString(),

                CertificateType = RequiredCertificate.ToString(),
                CertificateExpiry = Unit1CertExpiry,

                OdometerKm = PowerUnitOdometerKm,

                RucPurchasedDate = Unit1RucPurchasedDate,
                RucDistancePurchasedKm = Unit1RucDistancePurchasedKm,
                RucLicenceStartKm = Unit1RucLicenceStartKm,
                RucLicenceEndKm = Unit1RucLicenceEndKm
            });
        }
        else
        {
            units.Add(new CreateVehicleUnitRequest
            {
                UnitNumber = 2,
                Kind = "Trailer",
                VehicleType = SelectedVehicleType?.Value.ToString() ?? "",

                Rego = Unit2Rego,
                RegoExpiry = Unit2RegoExpiry,

                Make = Unit2Make,
                Model = Unit2Model,
                Year = Unit2Year ?? 0,

                Configuration = IsHeavyTrailer
                    ? SelectedHeavyConfig?.Value.ToString()
                    : SelectedLightConfig?.Value.ToString(),

                CertificateType = RequiredCertificate.ToString(),
                CertificateExpiry = Unit2CertExpiry,

                OdometerKm = Trailer1OdometerKm,

                RucPurchasedDate = Unit2RucPurchasedDate,
                RucDistancePurchasedKm = Unit2RucDistancePurchasedKm,
                RucLicenceStartKm = Unit2RucLicenceStartKm,
                RucLicenceEndKm = Unit2RucLicenceEndKm
            });

            if (IsBTrain)
            {
                units.Add(new CreateVehicleUnitRequest
                {
                    UnitNumber = 3,
                    Kind = "Trailer",
                    VehicleType = SelectedVehicleType?.Value.ToString() ?? "",

                    Rego = Unit3Rego,
                    RegoExpiry = Unit3RegoExpiry,

                    Make = Unit3Make,
                    Model = Unit3Model,
                    Year = Unit3Year ?? 0,

                    Configuration = SelectedHeavyConfig?.Value.ToString(),

                    CertificateType = RequiredCertificate.ToString(),
                    CertificateExpiry = Unit3CertExpiry,

                    OdometerKm = Trailer2OdometerKm,

                    RucPurchasedDate = Unit3RucPurchasedDate,
                    RucDistancePurchasedKm = Unit3RucDistancePurchasedKm,
                    RucLicenceStartKm = Unit3RucLicenceStartKm,
                    RucLicenceEndKm = Unit3RucLicenceEndKm
                });
            }
        }

        var request = new CreateVehicleSetRequest
        {
            Units = units
        };

        var result = await _vehiclesApiService.CreateVehicleSetAsync(request);

        if (result.AssetsCreated <= 0)
            throw new Exception("Vehicle set was not created.");
    }

    #endregion
}