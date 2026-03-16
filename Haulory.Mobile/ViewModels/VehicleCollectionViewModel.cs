using Haulory.Application.Interfaces.Services;
using Haulory.Contracts.Vehicles;
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

    #endregion

    #region State

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

    #endregion

    #region Collections

    public ObservableCollection<VehicleListItemViewModel> Assets { get; } = new();

    #endregion

    #region Commands

    public ICommand GoToNewVehicleCommand { get; }
    public ICommand EditVehicleCommand { get; }
    public ICommand RefreshCommand { get; }

    #endregion

    #region Constructor

    public VehicleCollectionViewModel(
        VehiclesApiService vehiclesApiService,
        JobsApiService jobsApiService,
        ISessionService session,
        IFeatureAccessService featureAccessService)
        : base(featureAccessService)
    {
        _vehiclesApiService = vehiclesApiService;
        _jobsApiService = jobsApiService;
        _session = session;

        GoToNewVehicleCommand = new Command(async () =>
        {
            if (!IsMainUser)
                return;

            await NavigateToFeatureAsync(AppFeature.AddVehicle, nameof(NewVehiclePage));
        });

        EditVehicleCommand = new Command<VehicleListItemViewModel>(async item =>
        {
            if (!IsMainUser)
                return;

            if (item == null || item.Id == Guid.Empty)
                return;

            if (!await EnsureFeatureEnabledAsync(AppFeature.AddVehicle))
                return;

            await Shell.Current.GoToAsync(nameof(NewVehiclePage), new Dictionary<string, object>
            {
                ["vehicleId"] = item.Id
            });
        });

        RefreshCommand = new Command(async () => await LoadAsync());
    }

    #endregion

    #region Load

    public async Task LoadAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
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
                    .Where(j => j.AssignedToUserId == accountId && j.VehicleAssetId.HasValue && j.VehicleAssetId.Value != Guid.Empty)
                    .Select(j => j.VehicleAssetId!.Value)
                    .Distinct()
                    .ToHashSet();

                visibleVehicles = allVehicles
                    .Where(v => assignedVehicleIds.Contains(v.Id));
            }

            foreach (var vehicle in visibleVehicles
                         .OrderBy(v => v.Rego)
                         .ThenBy(v => v.Make)
                         .ThenBy(v => v.Model))
            {
                Assets.Add(VehicleListItemViewModel.FromDto(vehicle));
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion

    #region Private Helpers

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