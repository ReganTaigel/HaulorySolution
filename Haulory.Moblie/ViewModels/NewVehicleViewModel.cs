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

    // COF/WOF Expiry (per asset)
    private DateTime? _unit1CertExpiry;
    private DateTime? _unit2CertExpiry;
    private DateTime? _unit3CertExpiry;

    // Make and Model
    private string _unit1Make = string.Empty;
    private string _unit1Model = string.Empty;

    private string _unit2Make = string.Empty;
    private string _unit2Model = string.Empty;

    private string _unit3Make = string.Empty;
    private string _unit3Model = string.Empty;

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
            if (RequiresFuelSelection && SelectedFuelType == null) return false;

            // Odo required only when OdoCount > 0, and only after odo step is visible
            if (OdoCount > 0)
            {
                if (!ShowOdometers) return false;

                if (OdoCount >= 1 && PowerUnitOdometerKm == null) return false;
                if (OdoCount >= 2 && Trailer1OdometerKm == null) return false;
                if (OdoCount >= 3 && Trailer2OdometerKm == null) return false;
            }

            return true;
        }
    }

    public bool ShowSaveButton =>
        // show once last required step is visible
        (OdoCount == 0 && CertificateComplete) ||
        (OdoCount > 0 && ShowOdometers);

    private async Task ExecuteSaveVehicleAsync()
    {
        if (!CanSaveVehicle) return;

        try
        {
            _isSaving = true;
            RefreshSaveState();

            await SaveVehicleAsync();

            await Shell.Current.DisplayAlert("Saved", "Vehicle saved successfully.", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Save failed", ex.Message, "OK");
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
            new(VehicleType.TruckClass4, "Truck (Class 4)"),

            new(VehicleType.TrailerClass3, "Trailer (Class 3)"),
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
            new(VehicleConfiguration.CurtainSiderTrailer, "Curtainsider Trailer"),
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

            Unit1Rego = Unit2Rego = Unit3Rego = string.Empty;
            Unit1RegoExpiry = Unit2RegoExpiry = Unit3RegoExpiry = null;

            Unit1Make = Unit1Model = string.Empty;
            Unit2Make = Unit2Model = string.Empty;
            Unit3Make = Unit3Model = string.Empty;

            Unit1CertExpiry = Unit2CertExpiry = Unit3CertExpiry = null;

            ResetOdometers();

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

            // if config changes, B-Train might turn on/off, so clear extra trailer fields
            if (!IsBTrain)
            {
                Unit2Rego = string.Empty;
                Unit2RegoExpiry = null;
                Unit2Make = string.Empty;
                Unit2Model = string.Empty;
                Unit2CertExpiry = null;

                Unit3Rego = string.Empty;
                Unit3RegoExpiry = null;
                Unit3Make = string.Empty;
                Unit3Model = string.Empty;
                Unit3CertExpiry = null;
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

    #region Make and Model

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

    public bool ShowMakeModel => RegoExpiryComplete;

    public bool MakeModelComplete
    {
        get
        {
            if (!ShowMakeModel) return false;

            if (string.IsNullOrWhiteSpace(Unit1Make) || string.IsNullOrWhiteSpace(Unit1Model))
                return false;

            if (RegoCount >= 2 &&
                (string.IsNullOrWhiteSpace(Unit2Make) || string.IsNullOrWhiteSpace(Unit2Model)))
                return false;

            if (RegoCount >= 3 &&
                (string.IsNullOrWhiteSpace(Unit3Make) || string.IsNullOrWhiteSpace(Unit3Model)))
                return false;

            return true;
        }
    }

    public string Unit1MakeModelLabel => "Make & model";
    public string Unit2MakeModelLabel => "Trailer 1 make & model";
    public string Unit3MakeModelLabel => "Trailer 2 make & model";

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

    #region Flow / Visibility (UPDATED)

    public bool HasVehicleType => SelectedVehicleType != null;

    // powered assets (need fuel + odo)
    public bool IsPoweredVehicle =>
        SelectedVehicleType?.Value == VehicleType.Car ||
        SelectedVehicleType?.Value == VehicleType.Ute ||
        SelectedVehicleType?.Value == VehicleType.TruckClass2 ||
        SelectedVehicleType?.Value == VehicleType.TruckClass4;

    public bool RequiresFuelSelection => IsPoweredVehicle;

    public bool IsTrailerAsset =>
        SelectedVehicleType?.Value == VehicleType.LightVehicleTrailer ||
        SelectedVehicleType?.Value == VehicleType.TrailerClass3 ||
        SelectedVehicleType?.Value == VehicleType.TrailerClass5;

    public bool IsLightTrailer =>
        SelectedVehicleType?.Value == VehicleType.LightVehicleTrailer;

    public bool IsHeavyTrailer =>
        SelectedVehicleType?.Value == VehicleType.TrailerClass3 ||
        SelectedVehicleType?.Value == VehicleType.TrailerClass5;

    public bool IsBTrain =>
        SelectedVehicleType?.Value == VehicleType.TrailerClass5 &&
        SelectedHeavyConfig?.Value == VehicleConfiguration.BTrainCurtainSider;

    public bool ShowLightTrailerConfig =>
        SelectedVehicleType?.Value == VehicleType.LightVehicleTrailer;

    public bool ShowClass4UnitType =>
        SelectedVehicleType?.Value == VehicleType.TruckClass4;

    public bool ShowHeavyConfig =>
        SelectedVehicleType?.Value == VehicleType.TrailerClass5;

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

    // How many regos this page is collecting
    public int RegoCount
    {
        get
        {
            if (!HasVehicleType) return 0;

            // B-Train (TrailerClass5 + BTrain config) = 2 trailers
            if (IsBTrain) return 2;

            // Everything else = single asset
            return 1;
        }
    }

    // UPDATED: ODO RULES
    // - Powered vehicles: 1 odo
    // - Light trailers: 0 odo
    // - Heavy trailers: 1 odo (per trailer); B-train: 2 odos
    public int OdoCount
    {
        get
        {
            if (!HasVehicleType) return 0;

            if (IsPoweredVehicle) return 1;

            if (IsLightTrailer) return 0;

            if (IsHeavyTrailer)
                return IsBTrain ? 2 : 1;

            return 0;
        }
    }

    public bool RegoComplete
    {
        get
        {
            if (!ReadyForRego) return false;

            if (string.IsNullOrWhiteSpace(Unit1Rego)) return false;
            if (RegoCount >= 2 && string.IsNullOrWhiteSpace(Unit2Rego)) return false;

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

            return true;
        }
    }

    public bool ShowCertificateStep => MakeModelComplete;

    public int CertificateCount => RegoCount;

    public bool ShowUnit1Cert => ShowCertificateStep;
    public bool ShowUnit2Cert => ShowCertificateStep && CertificateCount >= 2;
    public bool ShowUnit3Cert => false;

    public string Unit1CertLabel => $"{Unit1Label} {CertificateName} expiry";
    public string Unit2CertLabel => $"{Unit2Label} {CertificateName} expiry";
    public string Unit3CertLabel => string.Empty;

    public bool CertificateComplete
    {
        get
        {
            if (!ShowCertificateStep) return false;

            if (Unit1CertExpiry == null) return false;
            if (CertificateCount >= 2 && Unit2CertExpiry == null) return false;

            return true;
        }
    }

    // UPDATED: Fuel only for powered vehicles
    public bool ShowFuelType => CertificateComplete && IsPoweredVehicle;

    // UPDATED: Odometers:
    // - Powered vehicles: after fuel
    // - Heavy trailers: after certificate (no fuel step)
    public bool ShowOdometers =>
        CertificateComplete &&
        (
            (IsPoweredVehicle && SelectedFuelType != null) ||
            IsHeavyTrailer
        );

    public bool ShowUnit1Rego => ReadyForRego;
    public bool ShowUnit2Rego => ReadyForRego && RegoCount >= 2;
    public bool ShowUnit3Rego => false;

    public string Unit1Label => RegoCount >= 1 ? (IsTrailerAsset ? "Trailer rego" : "Vehicle rego") : string.Empty;
    public string Unit2Label => RegoCount >= 2 ? "Trailer 2 rego" : string.Empty;
    public string Unit3Label => string.Empty;

    public bool ShowPowerUnitOdo => ShowOdometers && OdoCount >= 1;
    public bool ShowTrailer1Odo => ShowOdometers && OdoCount >= 2; // B-train second trailer odo
    public bool ShowTrailer2Odo => false;

    public string PowerUnitOdoLabel =>
        IsPoweredVehicle ? "Odometer (km)" : "Trailer odometer (km)";

    public string Trailer1OdoLabel => "Trailer 2 odometer (km)";
    public string Trailer2OdoLabel => string.Empty;

    private void RaiseAllVisibility()
    {
        OnPropertyChanged(nameof(HasVehicleType));
        OnPropertyChanged(nameof(IsPoweredVehicle));
        OnPropertyChanged(nameof(RequiresFuelSelection));
        OnPropertyChanged(nameof(IsTrailerAsset));
        OnPropertyChanged(nameof(IsLightTrailer));
        OnPropertyChanged(nameof(IsHeavyTrailer));
        OnPropertyChanged(nameof(IsBTrain));

        OnPropertyChanged(nameof(ShowLightTrailerConfig));
        OnPropertyChanged(nameof(ShowClass4UnitType));
        OnPropertyChanged(nameof(ShowHeavyConfig));

        OnPropertyChanged(nameof(ReadyForRego));
        OnPropertyChanged(nameof(IsHeavyVehicle));

        OnPropertyChanged(nameof(RequiredCertificate));
        OnPropertyChanged(nameof(CertificateName));
        OnPropertyChanged(nameof(CertificatePluralName));

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

        RefreshSaveState();
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

    #region Save (ASSETS) - UPDATED

    public async Task SaveVehicleAsync()
    {
        if (SelectedVehicleType == null)
            throw new InvalidOperationException("Vehicle type is required.");

        // Fuel required only for powered vehicles
        if (IsPoweredVehicle && SelectedFuelType == null)
            throw new InvalidOperationException("Fuel type is required for powered vehicles.");

        var setId = Guid.NewGuid();
        var certType = RequiredCertificate;

        var assets = new List<VehicleAsset>();

        // Powered vehicle
        if (IsPoweredVehicle)
        {
            assets.Add(new VehicleAsset
            {
                VehicleSetId = setId,
                Kind = AssetKind.PowerUnit,
                VehicleType = SelectedVehicleType.Value,
                FuelType = SelectedFuelType!.Value,

                Rego = Unit1Rego.Trim(),
                RegoExpiry = Unit1RegoExpiry,

                Make = Unit1Make.Trim(),
                Model = Unit1Model.Trim(),

                CertificateType = certType,
                CertificateExpiry = Unit1CertExpiry,

                OdometerKm = PowerUnitOdometerKm
            });
        }
        else
        {
            // Trailer #1
            assets.Add(new VehicleAsset
            {
                VehicleSetId = setId,
                Kind = AssetKind.Trailer,
                VehicleType = SelectedVehicleType.Value,

                Rego = Unit1Rego.Trim(),
                RegoExpiry = Unit1RegoExpiry,

                Make = Unit1Make.Trim(),
                Model = Unit1Model.Trim(),

                CertificateType = certType,
                CertificateExpiry = Unit1CertExpiry,

                // Light trailers: no odo
                // Heavy trailers: use PowerUnitOdometerKm as "first trailer odo" field
                OdometerKm = IsHeavyTrailer ? PowerUnitOdometerKm : null
            });

            // B-Train: Trailer #2
            if (IsBTrain)
            {
                assets.Add(new VehicleAsset
                {
                    VehicleSetId = setId,
                    Kind = AssetKind.Trailer,
                    VehicleType = SelectedVehicleType.Value,

                    Rego = Unit2Rego.Trim(),
                    RegoExpiry = Unit2RegoExpiry,

                    Make = Unit2Make.Trim(),
                    Model = Unit2Model.Trim(),

                    CertificateType = certType,
                    CertificateExpiry = Unit2CertExpiry,

                    OdometerKm = Trailer1OdometerKm
                });
            }
        }

        await _vehicleRepository.AddRangeAsync(assets);
    }

    #endregion
}
