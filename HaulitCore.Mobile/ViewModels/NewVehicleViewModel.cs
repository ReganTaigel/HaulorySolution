using HaulitCore.Contracts.Vehicles;
using HaulitCore.Domain.Enums;
using HaulitCore.Mobile.Diagnostics;
using HaulitCore.Mobile.Features;
using HaulitCore.Mobile.Features.Vehicles.NewVehicle;
using HaulitCore.Mobile.Services;
using HaulitCore.Mobile.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace HaulitCore.Mobile.ViewModels;

public class NewVehicleViewModel : BaseViewModel, IQueryAttributable
{
    #region Dependencies

    private readonly NewVehicleFormState _state = new();
    private readonly NewVehicleValidator _validator = new();
    private readonly NewVehicleEditorService _editorService;
    private readonly ICrashLogger _crashLogger;

    #endregion

    #region UI Enum:
    public enum HeavyTrailerGroup
    {
        Semi,
        Drawbar,
        BDouble
    }
    #endregion

    #region Constructor

    public NewVehicleViewModel(
        VehiclesApiService vehiclesApiService,
        IFeatureAccessService featureAccessService,
        ICrashLogger crashLogger)
        : base(featureAccessService)
    {
        _crashLogger = crashLogger;

        _editorService = new NewVehicleEditorService(
            vehiclesApiService,
            new NewVehicleRequestMapper());

        SaveVehicleCommand = new Command(async () => await ExecuteSaveVehicleAsync(), () => CanSaveVehicle);
        RefreshSaveState();
    }

    #endregion

    #region Collections
    public ObservableCollection<VehicleOption<HeavyTrailerGroup>> HeavyTrailerGroups { get; } =
    new()
    {
        new(HeavyTrailerGroup.Semi, "Semi-Trailer"),
        new(HeavyTrailerGroup.Drawbar, "A-Frame / Drawbar Trailer"),
        new(HeavyTrailerGroup.BDouble, "Fifth Wheel / B-Train")
    };

    private ObservableCollection<VehicleOption<VehicleConfiguration>> _filteredHeavyConfigurations = new();

    public ObservableCollection<VehicleOption<VehicleConfiguration>> FilteredHeavyConfigurations
    {
        get => _filteredHeavyConfigurations;
        set
        {
            _filteredHeavyConfigurations = value;
            OnPropertyChanged();
        }
    }
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
            new(VehicleConfiguration.SingleAxle, "Single Axle Trailer"),
            new(VehicleConfiguration.TandemAxle, "Tandem Axle Trailer"),
        };

    public ObservableCollection<VehicleOption<VehicleConfiguration>> HeavyConfigurations { get; } =
       new()
       {
            new(VehicleConfiguration.SemiCurtainsider, "Semi Trailer (Curtains)"),
            new(VehicleConfiguration.SemiFlatDeck, "Semi Trailer (Flat Deck)"),
            new(VehicleConfiguration.SemiRefrigerated, "Semi Trailer (Refrigerated)"),
            new(VehicleConfiguration.SemiTanker, "Semi Trailer (Tanker)"),
            new(VehicleConfiguration.SemiSkeleton, "Semi Trailer (Skeleton)"),

            new(VehicleConfiguration.DrawbarCurtainsider, "Curtainsider Trailer "),
            new(VehicleConfiguration.DrawbarFlatDeck, "Flat Deck"),
            new(VehicleConfiguration.DrawbarRefrigerated, "Refrigerated Trailer"),
            new(VehicleConfiguration.DrawbarTanker, "Tanker Trailer "),

            new(VehicleConfiguration.BDblCurtainsider, "Curtainsider Trailer "),
            new(VehicleConfiguration.BDblFlatDeck, "Flat Deck Trailer "),
            new(VehicleConfiguration.BDblRefrigerated, "Refrigerated Trailers "),
            new(VehicleConfiguration.BDblTanker, "Tanker Trailer "),
            new(VehicleConfiguration.BDblBottomDumper, "Belly Dump Trailer "),
            new(VehicleConfiguration.BDblSideTipper, "Side Tippers Trailer "),
       };

    public ObservableCollection<VehicleOption<PowerUnitBodyType>> PowerUnitBodyTypes { get; } =
        new()
        {
            new(PowerUnitBodyType.Curtainsider, "Rigid Truck (Curtains)"),
            new(PowerUnitBodyType.FlatDeck, "Rigid Truck (Flat Deck)"),
            new(PowerUnitBodyType.Refrigerated, "Rigid Truck (Refrigerated)"),
            new(PowerUnitBodyType.Tanker, "Rigid Truck (Tanker)"),
            new(PowerUnitBodyType.Tipper, "Rigid Truck (Tipper)"),
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

    #region State Properties

    public bool IsEditMode => _state.EditingVehicleId.HasValue;
    public string PageTitle => IsEditMode ? "Edit vehicle" : "New vehicle";
    public string SaveButtonText => IsEditMode ? "Update vehicle" : "Save vehicle";
    public bool IsAddVehicleVisible => IsFeatureVisible(AppFeature.AddVehicle);
    public bool IsAddVehicleEnabled => IsFeatureEnabled(AppFeature.AddVehicle);
    public bool CanSaveVehicleAction => IsAddVehicleEnabled && CanSaveVehicle;

    public ICommand SaveVehicleCommand { get; }
    public bool CanSaveVehicle => _validator.CanSave(_state);

    public bool ShowSaveButton =>
        IsAddVehicleVisible &&
        ((!ShowRucStep && ((OdoCount == 0 && CertificateComplete) || (OdoCount > 0 && ShowHubodometers)))
         || (ShowRucStep && RucComplete));

    #endregion

    #region Selection Properties
    private HeavyTrailerGroup? _selectedHeavyTrailerGroup;

    public VehicleOption<HeavyTrailerGroup>? SelectedHeavyTrailerGroup
    {
        get => HeavyTrailerGroups.FirstOrDefault(x => x.Value == _selectedHeavyTrailerGroup);
        set
        {
            if (_selectedHeavyTrailerGroup == value?.Value)
                return;

            _selectedHeavyTrailerGroup = value?.Value;
            _state.HeavyConfiguration = null;

            FilteredHeavyConfigurations = _selectedHeavyTrailerGroup switch
            {
                HeavyTrailerGroup.Semi => new ObservableCollection<VehicleOption<VehicleConfiguration>>
            {
                new(VehicleConfiguration.SemiCurtainsider, "Curtainsider"),
                new(VehicleConfiguration.SemiFlatDeck, "Flat deck"),
                new(VehicleConfiguration.SemiRefrigerated, "Refrigerated"),
                new(VehicleConfiguration.SemiTanker, "Tanker"),
                new(VehicleConfiguration.SemiSkeleton, "Skeleton"),
                new(VehicleConfiguration.SemiTipper, "Tipper"),
            },

                HeavyTrailerGroup.Drawbar => new ObservableCollection<VehicleOption<VehicleConfiguration>>
            {
                new(VehicleConfiguration.DrawbarCurtainsider, "Curtainsider"),
                new(VehicleConfiguration.DrawbarFlatDeck, "Flat deck"),
                new(VehicleConfiguration.DrawbarRefrigerated, "Refrigerated"),
                new(VehicleConfiguration.DrawbarTanker, "Tanker"),
                new(VehicleConfiguration.DrawbarTipper, "Tipper"),
            },

                HeavyTrailerGroup.BDouble => new ObservableCollection<VehicleOption<VehicleConfiguration>>
            {
                new(VehicleConfiguration.BDblCurtainsider, "Curtainsider"),
                new(VehicleConfiguration.BDblFlatDeck, "Flat deck"),
                new(VehicleConfiguration.BDblRefrigerated, "Refrigerated"),
                new(VehicleConfiguration.BDblTanker, "Tanker"),
                new(VehicleConfiguration.BDblBottomDumper, "Belly Dump"),
                new(VehicleConfiguration.BDblSideTipper, "Side Tipper"),
            },
                _ => new ObservableCollection<VehicleOption<VehicleConfiguration>>()
            };

            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedHeavyConfig));

            RefreshSaveState();
        }
    }
    public VehicleOption<VehicleType>? SelectedVehicleType
    {
        get => VehicleTypes.FirstOrDefault(x => x.Value == _state.VehicleType);
        set
        {
            _state.VehicleType = value?.Value;
            _state.LightConfiguration = null;
            _state.HeavyConfiguration = null;
            _selectedHeavyTrailerGroup = null;
            _state.PowerUnitBodyType = null;
            _state.FuelType = null;
            _state.Unit1Rego = _state.Unit2Rego = _state.Unit3Rego = string.Empty;
            _state.Unit1RegoExpiry = _state.Unit2RegoExpiry = _state.Unit3RegoExpiry = null;
            _state.Unit1Make = _state.Unit1Model = string.Empty;
            _state.Unit1Year = null;
            _state.Unit2Make = _state.Unit2Model = string.Empty;
            _state.Unit2Year = null;
            _state.Unit3Make = _state.Unit3Model = string.Empty;
            _state.Unit3Year = null;
            _state.Unit1CertExpiry = _state.Unit2CertExpiry = _state.Unit3CertExpiry = null;
            _state.ResetHubodometers();
            _state.ResetAllRuc();
            OnPropertyChanged();
            RaiseAllVisibility();
        }
    }

    public VehicleOption<VehicleConfiguration>? SelectedLightConfig
    {
        get => LightTrailerConfigurations.FirstOrDefault(x => x.Value == _state.LightConfiguration);
        set
        {
            _state.LightConfiguration = value?.Value;
            OnPropertyChanged();
            RaiseAllVisibility();
        }
    }

    public VehicleOption<PowerUnitBodyType>? SelectedPowerUnitBodyType
    {
        get => PowerUnitBodyTypes.FirstOrDefault(x => x.Value == _state.PowerUnitBodyType);
        set
        {
            _state.PowerUnitBodyType = value?.Value;
            OnPropertyChanged();
            RaiseAllVisibility();
        }
    }

    public VehicleOption<VehicleConfiguration>? SelectedHeavyConfig
    {
        get => FilteredHeavyConfigurations.FirstOrDefault(x => x.Value == _state.HeavyConfiguration);
        set
        {
            _state.HeavyConfiguration = value?.Value;

            if (!IsBTrain)
            {
                _state.Unit3Rego = string.Empty;
                _state.Unit3RegoExpiry = null;
                _state.Unit3Make = string.Empty;
                _state.Unit3Model = string.Empty;
                _state.Unit3Year = null;
                _state.Unit3CertExpiry = null;
                _state.Trailer2HubodometerKm = null;
                _state.ResetUnit3Ruc();
            }

            OnPropertyChanged();
            RaiseAllVisibility();
        }
    }

    public VehicleOption<FuelType>? SelectedFuelType
    {
        get => FuelTypes.FirstOrDefault(x => x.Value == _state.FuelType);
        set
        {
            _state.FuelType = value?.Value;

            if (!RequiresRuc)
                _state.ResetUnit1Ruc();

            OnPropertyChanged();
            OnPropertyChanged(nameof(RequiresRuc));
            OnPropertyChanged(nameof(FuelInfoText));
            RaiseAllVisibility();
        }
    }

    #endregion

    #region Form Properties

    public string Unit1Rego { get => _state.Unit1Rego; set { _state.Unit1Rego = value; OnPropertyChanged(); RaiseAllVisibility(); } }
    public string Unit2Rego { get => _state.Unit2Rego; set { _state.Unit2Rego = value; OnPropertyChanged(); RaiseAllVisibility(); } }
    public string Unit3Rego { get => _state.Unit3Rego; set { _state.Unit3Rego = value; OnPropertyChanged(); RaiseAllVisibility(); } }
    public DateTime? Unit1RegoExpiry { get => _state.Unit1RegoExpiry; set { _state.Unit1RegoExpiry = value; OnPropertyChanged(); RaiseAllVisibility(); } }
    public DateTime? Unit2RegoExpiry { get => _state.Unit2RegoExpiry; set { _state.Unit2RegoExpiry = value; OnPropertyChanged(); RaiseAllVisibility(); } }
    public DateTime? Unit3RegoExpiry { get => _state.Unit3RegoExpiry; set { _state.Unit3RegoExpiry = value; OnPropertyChanged(); RaiseAllVisibility(); } }

    public string Unit1Make { get => _state.Unit1Make; set { _state.Unit1Make = value; OnPropertyChanged(); RaiseAllVisibility(); } }
    public string Unit1Model { get => _state.Unit1Model; set { _state.Unit1Model = value; OnPropertyChanged(); RaiseAllVisibility(); } }
    public int? Unit1Year { get => _state.Unit1Year; set { _state.Unit1Year = value; OnPropertyChanged(); RaiseAllVisibility(); } }
    public string Unit2Make { get => _state.Unit2Make; set { _state.Unit2Make = value; OnPropertyChanged(); RaiseAllVisibility(); } }
    public string Unit2Model { get => _state.Unit2Model; set { _state.Unit2Model = value; OnPropertyChanged(); RaiseAllVisibility(); } }
    public int? Unit2Year { get => _state.Unit2Year; set { _state.Unit2Year = value; OnPropertyChanged(); RaiseAllVisibility(); } }
    public string Unit3Make { get => _state.Unit3Make; set { _state.Unit3Make = value; OnPropertyChanged(); RaiseAllVisibility(); } }
    public string Unit3Model { get => _state.Unit3Model; set { _state.Unit3Model = value; OnPropertyChanged(); RaiseAllVisibility(); } }
    public int? Unit3Year { get => _state.Unit3Year; set { _state.Unit3Year = value; OnPropertyChanged(); RaiseAllVisibility(); } }

    public bool ShowMakeModel => _validator.ShowMakeModel(_state);
    public bool MakeModelComplete => _validator.MakeModelComplete(_state);
    public string Unit1MakeModelLabel => "Unit 1 — make, model & year";
    public string Unit2MakeModelLabel => "Unit 2 — make, model & year";
    public string Unit3MakeModelLabel => "Unit 3 — make, model & year";

    public DateTime? Unit1CertExpiry { get => _state.Unit1CertExpiry; set { _state.Unit1CertExpiry = value; OnPropertyChanged(); RaiseAllVisibility(); } }
    public DateTime? Unit2CertExpiry { get => _state.Unit2CertExpiry; set { _state.Unit2CertExpiry = value; OnPropertyChanged(); RaiseAllVisibility(); } }
    public DateTime? Unit3CertExpiry { get => _state.Unit3CertExpiry; set { _state.Unit3CertExpiry = value; OnPropertyChanged(); RaiseAllVisibility(); } }

    public int? PowerUnitHubodometerKm { get => _state.PowerUnitHubodometerKm; set { _state.PowerUnitHubodometerKm = value; OnPropertyChanged(); RefreshSaveState(); } }
    public int? Trailer1HubodometerKm { get => _state.Trailer1HubodometerKm; set { _state.Trailer1HubodometerKm = value; OnPropertyChanged(); RefreshSaveState(); } }
    public int? Trailer2HubodometerKm { get => _state.Trailer2HubodometerKm; set { _state.Trailer2HubodometerKm = value; OnPropertyChanged(); RefreshSaveState(); } }

    public bool RequiresRuc => _validator.RequiresRuc(_state);
    public bool TrailerRequiresRuc => _validator.TrailerRequiresRuc(_state);
    public int RucCount => _validator.RucCount(_state);
    public bool ShowRucStep => _validator.ShowRucStep(_state);
    public bool ShowUnit1Ruc => _validator.ShowUnit1Ruc(_state);
    public bool ShowUnit2Ruc => _validator.ShowUnit2Ruc(_state);
    public bool ShowUnit3Ruc => _validator.ShowUnit3Ruc(_state);
    public string Unit1RucLabel => "Unit 1 distance-based road tax";
    public string Unit2RucLabel => "Unit 2 distance-based road tax";
    public string Unit3RucLabel => "Unit 3 distance-based road tax";
    public bool RucComplete => _validator.RucComplete(_state);

    public DateTime? Unit1RucPurchasedDate { get => _state.Unit1RucPurchasedDate; set { _state.Unit1RucPurchasedDate = value; OnPropertyChanged(); RefreshSaveState(); } }
    public int? Unit1RucDistancePurchasedKm { get => _state.Unit1RucDistancePurchasedKm; set { _state.Unit1RucDistancePurchasedKm = value; OnPropertyChanged(); RefreshSaveState(); } }
    public int? Unit1RucLicenceStartKm { get => _state.Unit1RucLicenceStartKm; set { _state.Unit1RucLicenceStartKm = value; OnPropertyChanged(); RefreshSaveState(); } }
    public int? Unit1RucLicenceEndKm { get => _state.Unit1RucLicenceEndKm; set { _state.Unit1RucLicenceEndKm = value; OnPropertyChanged(); RefreshSaveState(); } }
    public DateTime? Unit2RucPurchasedDate { get => _state.Unit2RucPurchasedDate; set { _state.Unit2RucPurchasedDate = value; OnPropertyChanged(); RefreshSaveState(); } }
    public int? Unit2RucDistancePurchasedKm { get => _state.Unit2RucDistancePurchasedKm; set { _state.Unit2RucDistancePurchasedKm = value; OnPropertyChanged(); RefreshSaveState(); } }
    public int? Unit2RucLicenceStartKm { get => _state.Unit2RucLicenceStartKm; set { _state.Unit2RucLicenceStartKm = value; OnPropertyChanged(); RefreshSaveState(); } }
    public int? Unit2RucLicenceEndKm { get => _state.Unit2RucLicenceEndKm; set { _state.Unit2RucLicenceEndKm = value; OnPropertyChanged(); RefreshSaveState(); } }
    public DateTime? Unit3RucPurchasedDate { get => _state.Unit3RucPurchasedDate; set { _state.Unit3RucPurchasedDate = value; OnPropertyChanged(); RefreshSaveState(); } }
    public int? Unit3RucDistancePurchasedKm { get => _state.Unit3RucDistancePurchasedKm; set { _state.Unit3RucDistancePurchasedKm = value; OnPropertyChanged(); RefreshSaveState(); } }
    public int? Unit3RucLicenceStartKm { get => _state.Unit3RucLicenceStartKm; set { _state.Unit3RucLicenceStartKm = value; OnPropertyChanged(); RefreshSaveState(); } }
    public int? Unit3RucLicenceEndKm { get => _state.Unit3RucLicenceEndKm; set { _state.Unit3RucLicenceEndKm = value; OnPropertyChanged(); RefreshSaveState(); } }
    public string FuelInfoText => _validator.FuelInfoText(_state);

    #endregion

    #region Derived Properties

    public bool HasVehicleType => _validator.HasVehicleType(_state);
    public bool IsPoweredVehicle => _validator.IsPoweredVehicle(_state);
    public bool IsTrailerAsset => _validator.IsTrailerAsset(_state);
    public bool IsLightTrailer => _validator.IsLightTrailer(_state);
    public bool IsHeavyTrailer => _validator.IsHeavyTrailer(_state);
    public bool IsBTrain => _validator.IsBTrain(_state);
    public bool ShowLightTrailerConfig => _validator.ShowLightTrailerConfig(_state);
    public bool ShowPowerUnitBodyType => _validator.ShowPowerUnitBodyType(_state);
    public bool ShowHeavyConfig => _validator.ShowHeavyConfig(_state);
    public bool ReadyForRego => _validator.ReadyForRego(_state);
    public bool IsHeavyVehicle => _validator.IsHeavyVehicle(_state);
    public ComplianceCertificateType RequiredCertificate => _validator.RequiredCertificate(_state);
    public string CertificateName => "Inspection";
    public string CertificatePluralName => "Inspection / certification";
    public int RegoCount => _validator.RegoCount(_state);
    public int OdoCount => _validator.OdoCount(_state);
    public bool RegoComplete => _validator.RegoComplete(_state);
    public bool ShowRegoExpiry => _validator.ShowRegoExpiry(_state);
    public bool RegoExpiryComplete => _validator.RegoExpiryComplete(_state);
    public bool ShowCertificateStep => _validator.ShowCertificateStep(_state);
    public int CertificateCount => _validator.CertificateCount(_state);
    public bool ShowUnit1Cert => _validator.ShowUnit1Cert(_state);
    public bool ShowUnit2Cert => _validator.ShowUnit2Cert(_state);
    public bool ShowUnit3Cert => _validator.ShowUnit3Cert(_state);
    public string Unit1Label => "Unit 1 registration";
    public string Unit2Label => "Unit 2 registration";
    public string Unit3Label => "Unit 3 registration";
    public string Unit1CertLabel => "Unit 1 inspection/certification expiry";
    public string Unit2CertLabel => "Unit 2 inspection/certification expiry";
    public string Unit3CertLabel => "Unit 3 inspection/certification expiry";
    public bool CertificateComplete => _validator.CertificateComplete(_state);
    public bool ShowFuelType => _validator.ShowFuelType(_state);
    public bool ShowHubodometers => _validator.ShowHubodometers(_state);
    public bool ShowUnit1Rego => _validator.ShowUnit1Rego(_state);
    public bool ShowUnit2Rego => _validator.ShowUnit2Rego(_state);
    public bool ShowUnit3Rego => _validator.ShowUnit3Rego(_state);
    public bool ShowPowerUnitOdo => _validator.ShowPowerUnitOdo(_state);
    public bool ShowTrailer1Odo => _validator.ShowTrailer1Odo(_state);
    public bool ShowTrailer2Odo => _validator.ShowTrailer2Odo(_state);
    public string PowerUnitOdoLabel => "Hubodometer";
    public string Trailer1OdoLabel => "Hubodometer (unit 2)";
    public string Trailer2OdoLabel => "Hubodometer (unit 3)";

    #endregion

    #region Public Methods

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("vehicleId", out var vehicleIdValue))
        {
            if (vehicleIdValue is Guid g)
                _state.EditingVehicleId = g;
            else if (vehicleIdValue is string s && Guid.TryParse(s, out var parsed))
                _state.EditingVehicleId = parsed;
        }

        RefreshSaveState();

        if (IsEditMode && !_state.IsLoadingExistingVehicle)
            _ = LoadExistingVehicleSafeAsync();
    }

    public async Task LoadAsync()
    {
        if (IsEditMode && _state.EditingVehicleId.HasValue && !_state.IsLoadingExistingVehicle)
            await LoadExistingVehicleSafeAsync();
    }

    public async Task SaveVehicleAsync() => await _editorService.SaveAsync(_state);

    #endregion

    #region Private Methods
    private HeavyTrailerGroup? GetHeavyTrailerGroup(VehicleConfiguration? config)
    {
        return config switch
        {
            VehicleConfiguration.SemiCurtainsider
            or VehicleConfiguration.SemiFlatDeck
            or VehicleConfiguration.SemiRefrigerated
            or VehicleConfiguration.SemiTanker
            or VehicleConfiguration.SemiSkeleton
            or VehicleConfiguration.SemiTipper
                => HeavyTrailerGroup.Semi,

            VehicleConfiguration.DrawbarCurtainsider
            or VehicleConfiguration.DrawbarFlatDeck
            or VehicleConfiguration.DrawbarRefrigerated
            or VehicleConfiguration.DrawbarTanker
            or VehicleConfiguration.DrawbarTipper
                => HeavyTrailerGroup.Drawbar,

            VehicleConfiguration.BDblCurtainsider
            or VehicleConfiguration.BDblFlatDeck
            or VehicleConfiguration.BDblRefrigerated
            or VehicleConfiguration.BDblTanker
            or VehicleConfiguration.BDblBottomDumper
            or VehicleConfiguration.BDblSideTipper
                => HeavyTrailerGroup.BDouble,

            _ => null
        };
    }
    private async Task UpdateVehicleAsync()
    {
        if (!_state.EditingVehicleId.HasValue)
            throw new Exception("Vehicle id is missing.");

        await _editorService.UpdateAsync(_state.EditingVehicleId.Value, _state);
    }

    private async Task ExecuteSaveVehicleAsync()
    {
        if (!await EnsureFeatureEnabledAsync(AppFeature.AddVehicle))
            return;

        if (!CanSaveVehicle)
            return;

        try
        {
            _state.IsSaving = true;
            RefreshSaveState();

            await SafeRunner.RunAsync(
                async () =>
                {
                    if (IsEditMode)
                        await UpdateVehicleAsync();
                    else
                        await SaveVehicleAsync();

                    await Shell.Current.DisplayAlertAsync(
                        "Saved",
                        IsEditMode ? "Vehicle updated successfully." : "Vehicle saved successfully.",
                        "OK");

                    await Shell.Current.GoToAsync($"//{nameof(VehicleCollectionPage)}");
                },
                _crashLogger,
                "NewVehicleViewModel.ExecuteSaveVehicleAsync",
                nameof(NewVehiclePage),
                metadataJson: $"{{\"IsEditMode\":{IsEditMode.ToString().ToLowerInvariant()},\"VehicleId\":\"{_state.EditingVehicleId}\"}}",
                onError: async ex =>
                {
                    await Shell.Current.DisplayAlertAsync("Save failed", ex.Message, "OK");
                });
        }
        finally
        {
            _state.IsSaving = false;
            RefreshSaveState();
        }
    }

    private async Task LoadExistingVehicleSafeAsync()
    {
        try
        {
            _state.IsLoadingExistingVehicle = true;

            await SafeRunner.RunAsync(
                async () => await LoadExistingVehicleAsync(),
                _crashLogger,
                "NewVehicleViewModel.LoadExistingVehicleSafeAsync",
                nameof(NewVehiclePage),
                metadataJson: $"{{\"VehicleId\":\"{_state.EditingVehicleId}\"}}",
                onError: async ex =>
                {
                    await Shell.Current.DisplayAlertAsync("Load failed", ex.Message, "OK");
                });
        }
        finally
        {
            _state.IsLoadingExistingVehicle = false;
        }
    }

    private async Task LoadExistingVehicleAsync()
    {
        if (!_state.EditingVehicleId.HasValue)
            return;

        var loadedState = await _editorService.LoadAsync(_state.EditingVehicleId.Value);
        CopyState(loadedState);
        RaiseAllVisibility();
    }

    private void CopyState(NewVehicleFormState loaded)
    {
        _state.EditingVehicleId = loaded.EditingVehicleId;

        // Preserve existing asset ids so update can mutate the full set
        // without losing audit identity.
        _state.Unit1Id = loaded.Unit1Id;
        _state.Unit2Id = loaded.Unit2Id;
        _state.Unit3Id = loaded.Unit3Id;

        _state.VehicleType = loaded.VehicleType;
        _state.LightConfiguration = loaded.LightConfiguration;
        _state.HeavyConfiguration = loaded.HeavyConfiguration;
        _state.PowerUnitBodyType = loaded.PowerUnitBodyType;
        _state.FuelType = loaded.FuelType;
        _state.Unit1Rego = loaded.Unit1Rego;
        _state.Unit2Rego = loaded.Unit2Rego;
        _state.Unit3Rego = loaded.Unit3Rego;
        _state.Unit1RegoExpiry = loaded.Unit1RegoExpiry;
        _state.Unit2RegoExpiry = loaded.Unit2RegoExpiry;
        _state.Unit3RegoExpiry = loaded.Unit3RegoExpiry;
        _state.PowerUnitHubodometerKm = loaded.PowerUnitHubodometerKm;
        _state.Trailer1HubodometerKm = loaded.Trailer1HubodometerKm;
        _state.Trailer2HubodometerKm = loaded.Trailer2HubodometerKm;
        _state.Unit1RucLicenceStartKm = loaded.Unit1RucLicenceStartKm;
        _state.Unit1RucLicenceEndKm = loaded.Unit1RucLicenceEndKm;
        _state.Unit2RucLicenceStartKm = loaded.Unit2RucLicenceStartKm;
        _state.Unit2RucLicenceEndKm = loaded.Unit2RucLicenceEndKm;
        _state.Unit3RucLicenceStartKm = loaded.Unit3RucLicenceStartKm;
        _state.Unit3RucLicenceEndKm = loaded.Unit3RucLicenceEndKm;
        _state.Unit1RucPurchasedDate = loaded.Unit1RucPurchasedDate;
        _state.Unit1RucDistancePurchasedKm = loaded.Unit1RucDistancePurchasedKm;
        _state.Unit2RucPurchasedDate = loaded.Unit2RucPurchasedDate;
        _state.Unit2RucDistancePurchasedKm = loaded.Unit2RucDistancePurchasedKm;
        _state.Unit3RucPurchasedDate = loaded.Unit3RucPurchasedDate;
        _state.Unit3RucDistancePurchasedKm = loaded.Unit3RucDistancePurchasedKm;
        _state.Unit1CertExpiry = loaded.Unit1CertExpiry;
        _state.Unit2CertExpiry = loaded.Unit2CertExpiry;
        _state.Unit3CertExpiry = loaded.Unit3CertExpiry;
        _state.Unit1Make = loaded.Unit1Make;
        _state.Unit1Model = loaded.Unit1Model;
        _state.Unit1Year = loaded.Unit1Year;
        _state.Unit2Make = loaded.Unit2Make;
        _state.Unit2Model = loaded.Unit2Model;
        _state.Unit2Year = loaded.Unit2Year;
        _state.Unit3Make = loaded.Unit3Make;
        _state.Unit3Model = loaded.Unit3Model;
        _state.Unit3Year = loaded.Unit3Year;

        _selectedHeavyTrailerGroup = GetHeavyTrailerGroup(loaded.HeavyConfiguration);
    }

    private void RefreshSaveState()
    {
        OnPropertyChanged(nameof(CanSaveVehicle));
        OnPropertyChanged(nameof(ShowSaveButton));
        OnPropertyChanged(nameof(IsAddVehicleVisible));
        OnPropertyChanged(nameof(IsAddVehicleEnabled));
        OnPropertyChanged(nameof(CanSaveVehicleAction));
        OnPropertyChanged(nameof(IsEditMode));
        OnPropertyChanged(nameof(PageTitle));
        OnPropertyChanged(nameof(SaveButtonText));
        (SaveVehicleCommand as Command)?.ChangeCanExecute();
    }

    private void RaiseAllVisibility()
    {
        OnPropertyChanged(nameof(SelectedVehicleType));
        OnPropertyChanged(nameof(SelectedLightConfig));
        OnPropertyChanged(nameof(SelectedPowerUnitBodyType));
        OnPropertyChanged(nameof(SelectedHeavyConfig));
        OnPropertyChanged(nameof(SelectedFuelType));
        OnPropertyChanged(nameof(Unit1Rego));
        OnPropertyChanged(nameof(Unit2Rego));
        OnPropertyChanged(nameof(Unit3Rego));
        OnPropertyChanged(nameof(Unit1RegoExpiry));
        OnPropertyChanged(nameof(Unit2RegoExpiry));
        OnPropertyChanged(nameof(Unit3RegoExpiry));
        OnPropertyChanged(nameof(Unit1Make));
        OnPropertyChanged(nameof(Unit2Make));
        OnPropertyChanged(nameof(Unit3Make));
        OnPropertyChanged(nameof(Unit1Model));
        OnPropertyChanged(nameof(Unit2Model));
        OnPropertyChanged(nameof(Unit3Model));
        OnPropertyChanged(nameof(Unit1Year));
        OnPropertyChanged(nameof(Unit2Year));
        OnPropertyChanged(nameof(Unit3Year));
        OnPropertyChanged(nameof(Unit1CertExpiry));
        OnPropertyChanged(nameof(Unit2CertExpiry));
        OnPropertyChanged(nameof(Unit3CertExpiry));
        OnPropertyChanged(nameof(PowerUnitHubodometerKm));
        OnPropertyChanged(nameof(Trailer1HubodometerKm));
        OnPropertyChanged(nameof(Trailer2HubodometerKm));
        OnPropertyChanged(nameof(Unit1RucPurchasedDate));
        OnPropertyChanged(nameof(Unit1RucDistancePurchasedKm));
        OnPropertyChanged(nameof(Unit1RucLicenceStartKm));
        OnPropertyChanged(nameof(Unit1RucLicenceEndKm));
        OnPropertyChanged(nameof(Unit2RucPurchasedDate));
        OnPropertyChanged(nameof(Unit2RucDistancePurchasedKm));
        OnPropertyChanged(nameof(Unit2RucLicenceStartKm));
        OnPropertyChanged(nameof(Unit2RucLicenceEndKm));
        OnPropertyChanged(nameof(Unit3RucPurchasedDate));
        OnPropertyChanged(nameof(Unit3RucDistancePurchasedKm));
        OnPropertyChanged(nameof(Unit3RucLicenceStartKm));
        OnPropertyChanged(nameof(Unit3RucLicenceEndKm));

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
        OnPropertyChanged(nameof(ShowHubodometers));
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
} 