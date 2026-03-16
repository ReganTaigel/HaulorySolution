using Haulory.Application.Interfaces.Services;
using Haulory.Contracts.Vehicles;
using Haulory.Mobile.Diagnostics;
using Haulory.Mobile.Features;
using Haulory.Mobile.Services;
using Haulory.Mobile.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Haulory.Mobile.ViewModels;

public class VehicleCollectionViewModel : BaseViewModel
{
    #region Dependencies

    private readonly VehiclesApiService _vehiclesApiService;
    private readonly JobsApiService _jobsApiService;
    private readonly ISessionService _session;
    private readonly ICrashLogger _crashLogger;

    #endregion

    #region Properties

    public bool IsSubUser =>
        _session.CurrentAccountId.HasValue &&
        _session.CurrentOwnerId.HasValue &&
        _session.CurrentOwnerId.Value != _session.CurrentAccountId.Value;

    public bool IsMainUser =>
        _session.CurrentAccountId.HasValue &&
        _session.CurrentOwnerId.HasValue &&
        _session.CurrentOwnerId.Value == _session.CurrentAccountId.Value;

    public bool IsVehiclesVisible => IsFeatureVisible(AppFeature.Vehicles);
    public bool IsVehiclesEnabled => IsFeatureEnabled(AppFeature.Vehicles);

    public bool IsAddVehicleVisible => IsFeatureVisible(AppFeature.AddVehicle) && IsMainUser;
    public bool IsAddVehicleEnabled => IsFeatureEnabled(AppFeature.AddVehicle) && IsMainUser;

    public ObservableCollection<VehicleListItemViewModel> Assets { get; } = new();

    #endregion

    #region Commands

    public ICommand GoToNewVehicleCommand { get; }
    public ICommand EditVehicleCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand ToggleVehicleExpandedCommand { get; }

    #endregion

    #region Constructors

    public VehicleCollectionViewModel(
        VehiclesApiService vehiclesApiService,
        JobsApiService jobsApiService,
        ISessionService session,
        IFeatureAccessService featureAccessService,
        ICrashLogger crashLogger)
        : base(featureAccessService)
    {
        _vehiclesApiService = vehiclesApiService;
        _jobsApiService = jobsApiService;
        _session = session;
        _crashLogger = crashLogger;

        GoToNewVehicleCommand = new Command(async () =>
        {
            if (!IsMainUser)
                return;

            await SafeRunner.RunAsync(
                async () => await NavigateToFeatureAsync(AppFeature.AddVehicle, nameof(NewVehiclePage)),
                _crashLogger,
                "VehicleCollectionViewModel.GoToNewVehicleCommand",
                nameof(VehicleCollectionPage));
        });

        EditVehicleCommand = new Command<VehicleListItemViewModel>(async item =>
        {
            if (!IsMainUser)
                return;

            if (item == null || item.Id == Guid.Empty)
                return;

            await SafeRunner.RunAsync(
                async () =>
                {
                    if (!await EnsureFeatureEnabledAsync(AppFeature.AddVehicle))
                        return;

                    await Shell.Current.GoToAsync(nameof(NewVehiclePage), new Dictionary<string, object>
                    {
                        ["vehicleId"] = item.Id
                    });
                },
                _crashLogger,
                "VehicleCollectionViewModel.EditVehicleCommand",
                nameof(VehicleCollectionPage));
        });

        RefreshCommand = new Command(async () => await LoadAsync());

        ToggleVehicleExpandedCommand = new Command<VehicleListItemViewModel>(item =>
        {
            if (item == null)
                return;

            foreach (var vehicle in Assets)
            {
                if (!ReferenceEquals(vehicle, item))
                    vehicle.IsExpanded = false;
            }

            item.IsExpanded = !item.IsExpanded;
        });
    }

    #endregion

    #region Public Methods

    public async Task LoadAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            await SafeRunner.RunAsync(
                async () =>
                {
                    Assets.Clear();

                    if (!IsFeatureEnabled(AppFeature.Vehicles))
                    {
                        RefreshFeatureBindings();
                        return;
                    }

                    if (!_session.IsAuthenticated)
                        await _session.RestoreAsync();

                    var ownerUserId = _session.CurrentOwnerId ?? Guid.Empty;
                    var accountId = _session.CurrentAccountId ?? Guid.Empty;

                    if (ownerUserId == Guid.Empty || accountId == Guid.Empty)
                    {
                        RefreshFeatureBindings();
                        return;
                    }

                    OnPropertyChanged(nameof(IsMainUser));
                    OnPropertyChanged(nameof(IsSubUser));
                    RefreshFeatureBindings();

                    var allVehicles = await _vehiclesApiService.GetVehiclesAsync();

                    IEnumerable<VehicleDto> visibleVehicles = allVehicles;

                    if (IsSubUser)
                    {
                        var jobs = await _jobsApiService.GetActiveJobsAsync();

                        var assignedVehicleIds = jobs
                            .Where(j => j.AssignedToUserId == accountId &&
                                        j.VehicleAssetId.HasValue &&
                                        j.VehicleAssetId.Value != Guid.Empty)
                            .Select(j => j.VehicleAssetId!.Value)
                            .Distinct()
                            .ToHashSet();

                        visibleVehicles = allVehicles
                            .Where(v => assignedVehicleIds.Contains(v.Id));
                    }

                    var orderedVehicles = visibleVehicles
                        .OrderBy(v => GetVehicleSortOrder(v))
                        .ThenBy(v => v.Rego ?? string.Empty)
                        .ThenBy(v => v.Make ?? string.Empty)
                        .ThenBy(v => v.Model ?? string.Empty)
                        .ToList();

                    foreach (var vehicle in orderedVehicles)
                    {
                        Assets.Add(VehicleListItemViewModel.FromDto(vehicle));
                    }
                },
                _crashLogger,
                "VehicleCollectionViewModel.LoadAsync",
                nameof(VehicleCollectionPage),
                onError: async _ =>
                {
                    await Shell.Current.DisplayAlertAsync("Error", "Unable to load vehicles right now.", "OK");
                });
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion

    #region Private Methods

    private static int GetVehicleSortOrder(VehicleDto vehicle)
    {
        var type = (vehicle.VehicleType ?? vehicle.Kind ?? string.Empty)
            .Trim()
            .ToLowerInvariant();

        var isTrailer = type.Contains("trailer");
        var isLight = type.Contains("light");
        var isHeavy = type.Contains("heavy");

        if (!isTrailer && isLight)
            return 0;

        if (!isTrailer && isHeavy)
            return 1;

        if (isTrailer && isLight)
            return 2;

        if (isTrailer && isHeavy)
            return 3;

        return 4;
    }

    private void RefreshFeatureBindings()
    {
        OnPropertyChanged(nameof(IsVehiclesVisible));
        OnPropertyChanged(nameof(IsVehiclesEnabled));
        OnPropertyChanged(nameof(IsAddVehicleVisible));
        OnPropertyChanged(nameof(IsAddVehicleEnabled));
    }

    private async Task NavigateToFeatureAsync(AppFeature feature, string route)
    {
        if (!await EnsureFeatureEnabledAsync(feature))
            return;

        await Shell.Current.GoToAsync(route);
    }

    #endregion
}