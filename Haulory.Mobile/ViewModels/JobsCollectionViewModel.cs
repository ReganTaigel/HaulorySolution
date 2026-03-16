using System.Collections.ObjectModel;
using System.Windows.Input;
using Haulory.Application.Interfaces.Services;
using Haulory.Contracts.Drivers;
using Haulory.Contracts.Jobs;
using Haulory.Contracts.Vehicles;
using Haulory.Mobile.Diagnostics;
using Haulory.Mobile.Features;
using Haulory.Mobile.Services;
using Haulory.Mobile.Views;
using Microsoft.Maui.Controls;

namespace Haulory.Mobile.ViewModels;

public class JobsCollectionViewModel : BaseViewModel
{
    #region Dependencies

    private readonly JobsApiService _jobsApiService;
    private readonly DriversApiService _driversApiService;
    private readonly VehiclesApiService _vehiclesApiService;
    private readonly ISessionService _session;
    private readonly ICrashLogger _crashLogger;

    #endregion

    #region State

    private string _mainUserDisplayName = "Main User";

    #endregion

    #region Collections

    public ObservableCollection<JobListItemViewModel> Jobs { get; } = new();
    public ObservableCollection<JobGroupViewModel> JobGroups { get; } = new();

    #endregion

    #region Commands

    public ICommand AddJobCommand { get; }
    public ICommand EditJobCommand { get; }
    public ICommand MoveUpCommand { get; }
    public ICommand MoveDownCommand { get; }
    public ICommand SignDeliveryCommand { get; }

    #endregion

    #region Feature Access

    public bool IsSubUser =>
        _session.CurrentAccountId.HasValue &&
        _session.CurrentOwnerId.HasValue &&
        _session.CurrentOwnerId.Value != _session.CurrentAccountId.Value;

    public bool IsMainUser =>
        _session.CurrentAccountId.HasValue &&
        _session.CurrentOwnerId.HasValue &&
        _session.CurrentOwnerId.Value == _session.CurrentAccountId.Value;

    public bool IsJobsVisible => IsFeatureVisible(AppFeature.Jobs);
    public bool IsJobsEnabled => IsFeatureEnabled(AppFeature.Jobs);

    public bool IsAddJobVisible => IsFeatureVisible(AppFeature.AddJob) && IsMainUser;
    public bool IsAddJobEnabled => IsFeatureEnabled(AppFeature.AddJob) && IsMainUser;

    public bool IsDeliverySignatureVisible => IsFeatureVisible(AppFeature.DeliverySignature);
    public bool IsDeliverySignatureEnabled => IsFeatureEnabled(AppFeature.DeliverySignature);

    #endregion

    #region Constructor

    public JobsCollectionViewModel(
        JobsApiService jobsApiService,
        DriversApiService driversApiService,
        VehiclesApiService vehiclesApiService,
        ISessionService session,
        IFeatureAccessService featureAccessService,
        ICrashLogger crashLogger)
        : base(featureAccessService)
    {
        _jobsApiService = jobsApiService;
        _driversApiService = driversApiService;
        _vehiclesApiService = vehiclesApiService;
        _session = session;
        _crashLogger = crashLogger;

        AddJobCommand = new Command(async () =>
        {
            if (!IsMainUser)
                return;

            await SafeRunner.RunAsync(
                async () =>
                {
                    if (!await EnsureFeatureEnabledAsync(AppFeature.AddJob))
                        return;

                    if (!await HasAssignableVehicleAsync())
                    {
                        await Shell.Current.DisplayAlertAsync(
                            "Vehicle required",
                            "You need at least one vehicle before you can add a job.",
                            "OK");

                        return;
                    }

                    await Shell.Current.GoToAsync(nameof(NewJobPage));
                },
                _crashLogger,
                "JobsCollectionViewModel.AddJobCommand",
                nameof(JobsCollectionPage));
        });

        EditJobCommand = new Command<JobListItemViewModel>(async item =>
        {
            if (item?.Job == null)
                return;

            await SafeRunner.RunAsync(
                async () =>
                {
                    if (IsMainUser)
                    {
                        if (!await EnsureFeatureEnabledAsync(AppFeature.AddJob))
                            return;

                        await Shell.Current.GoToAsync(nameof(NewJobPage), new Dictionary<string, object>
                        {
                            ["jobId"] = item.Job.Id
                        });

                        return;
                    }

                    await Shell.Current.GoToAsync(nameof(NewJobPage), new Dictionary<string, object>
                    {
                        ["jobId"] = item.Job.Id,
                        ["pickupOnly"] = true
                    });
                },
                _crashLogger,
                "JobsCollectionViewModel.EditJobCommand",
                nameof(JobsCollectionPage),
                metadataJson: $"{{\"JobId\":\"{item.Job.Id}\",\"IsMainUser\":{IsMainUser.ToString().ToLowerInvariant()}}}");
        });

        SignDeliveryCommand = new Command<JobListItemViewModel>(async item =>
        {
            if (item?.Job == null)
                return;

            await SafeRunner.RunAsync(
                async () =>
                {
                    if (!await EnsureFeatureEnabledAsync(AppFeature.DeliverySignature))
                        return;

                    await Shell.Current.GoToAsync(
                        $"{nameof(DeliverySignaturePage)}?jobId={item.Job.Id}");
                },
                _crashLogger,
                "JobsCollectionViewModel.SignDeliveryCommand",
                nameof(JobsCollectionPage),
                metadataJson: $"{{\"JobId\":\"{item.Job.Id}\"}}");
        });

        MoveUpCommand = new Command<JobListItemViewModel>(async item =>
        {
            if (!IsMainUser)
                return;

            await SafeRunner.RunAsync(
                async () =>
                {
                    if (!await EnsureFeatureEnabledAsync(AppFeature.Jobs))
                        return;

                    await Shell.Current.DisplayAlertAsync(
                        "Not moved",
                        "Job reordering has not been moved to the API yet.",
                        "OK");
                },
                _crashLogger,
                "JobsCollectionViewModel.MoveUpCommand",
                nameof(JobsCollectionPage),
                metadataJson: item?.Job == null ? null : $"{{\"JobId\":\"{item.Job.Id}\"}}");
        });

        MoveDownCommand = new Command<JobListItemViewModel>(async item =>
        {
            if (!IsMainUser)
                return;

            await SafeRunner.RunAsync(
                async () =>
                {
                    if (!await EnsureFeatureEnabledAsync(AppFeature.Jobs))
                        return;

                    await Shell.Current.DisplayAlertAsync(
                        "Not moved",
                        "Job reordering has not been moved to the API yet.",
                        "OK");
                },
                _crashLogger,
                "JobsCollectionViewModel.MoveDownCommand",
                nameof(JobsCollectionPage),
                metadataJson: item?.Job == null ? null : $"{{\"JobId\":\"{item.Job.Id}\"}}");
        });
    }

    #endregion

    #region Public Methods

    public async Task LoadAsync()
    {
        await SafeRunner.RunAsync(
            async () =>
            {
                Jobs.Clear();
                JobGroups.Clear();

                if (!IsFeatureEnabled(AppFeature.Jobs))
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

                var allDrivers = (await _driversApiService.GetDriversAsync()).ToList();

                var mainDriver = allDrivers.FirstOrDefault(d =>
                    d.UserId.HasValue && d.UserId.Value == ownerUserId);

                if (mainDriver != null)
                {
                    _mainUserDisplayName =
                        !string.IsNullOrWhiteSpace(mainDriver.DisplayName)
                            ? mainDriver.DisplayName!
                            : $"{mainDriver.FirstName} {mainDriver.LastName}".Trim();

                    if (string.IsNullOrWhiteSpace(_mainUserDisplayName))
                        _mainUserDisplayName = "Main User";
                }
                else
                {
                    _mainUserDisplayName = "Main User";
                }

                var jobs = (await _jobsApiService.GetActiveJobsAsync()).ToList();
                jobs = jobs.OrderBy(j => j.SortOrder).ToList();

                var driverIds = jobs
                    .Where(j => j.DriverId.HasValue)
                    .Select(j => j.DriverId!.Value)
                    .Distinct()
                    .ToList();

                var vehicleIds = jobs
                    .Where(j => j.VehicleAssetId.HasValue)
                    .Select(j => j.VehicleAssetId!.Value)
                    .Distinct()
                    .ToList();

                var trailerIds = jobs
                    .SelectMany(j => j.TrailerAssetIds)
                    .Distinct()
                    .ToList();

                var driversById = new Dictionary<Guid, DriverDto>();

                foreach (var driver in allDrivers.Where(d => driverIds.Contains(d.Id)))
                    driversById[driver.Id] = driver;

                foreach (var id in driverIds.Where(id => !driversById.ContainsKey(id)))
                {
                    var driver = await _driversApiService.GetDriverByIdAsync(id);
                    if (driver != null)
                        driversById[id] = driver;
                }

                var assetsById = new Dictionary<Guid, VehicleDto>();
                foreach (var id in vehicleIds.Concat(trailerIds).Distinct())
                {
                    var asset = await _vehiclesApiService.GetVehicleByIdAsync(id);
                    if (asset != null)
                        assetsById[id] = asset;
                }

                var showPricing = IsMainUser;
                var items = new List<JobListItemViewModel>();

                foreach (var job in jobs)
                {
                    var driverName = "—";
                    if (job.DriverId is Guid did &&
                        driversById.TryGetValue(did, out var driver) &&
                        driver != null)
                    {
                        driverName = string.IsNullOrWhiteSpace(driver.DisplayName)
                            ? $"{driver.FirstName} {driver.LastName}".Trim()
                            : driver.DisplayName!;
                    }

                    var truck = "—";
                    if (job.VehicleAssetId is Guid vid &&
                        assetsById.TryGetValue(vid, out var vehicle) &&
                        vehicle != null)
                    {
                        truck = $"{vehicle.Make} {vehicle.Model} • {vehicle.Rego}".Trim();
                    }

                    var canShowSignDelivery =
                        IsSubUser
                            ? job.AssignedToUserId == accountId
                            : job.AssignedToUserId == null || job.AssignedToUserId == accountId;

                    items.Add(new JobListItemViewModel(
                        job,
                        driverName,
                        truck,
                        showPricing,
                        canShowSignDelivery));
                }

                if (IsSubUser)
                {
                    foreach (var item in items
                                 .Where(x => x.Job.AssignedToUserId == accountId)
                                 .OrderBy(x => x.Job.SortOrder))
                    {
                        Jobs.Add(item);
                    }
                }
                else
                {
                    BuildMainUserGroups(items, accountId);
                }

                OnPropertyChanged(nameof(IsMainUser));
                OnPropertyChanged(nameof(IsSubUser));
                RefreshFeatureBindings();
            },
            _crashLogger,
            "JobsCollectionViewModel.LoadAsync",
            nameof(JobsCollectionPage),
            onError: async ex =>
            {
                await Shell.Current.DisplayAlertAsync("Load failed", ex.Message, "OK");
            });
    }

    #endregion

    #region Private Methods

    private async Task<bool> HasAssignableVehicleAsync()
    {
        var vehicles = await _vehiclesApiService.GetVehiclesAsync();
        return vehicles.Any();
    }

    private void BuildMainUserGroups(List<JobListItemViewModel> items, Guid currentAccountId)
    {
        var mainGroup = new JobGroupViewModel(_mainUserDisplayName, isMainGroup: true);

        foreach (var item in items
                     .Where(x => x.Job.AssignedToUserId == null || x.Job.AssignedToUserId == currentAccountId)
                     .OrderBy(x => x.Job.SortOrder))
        {
            mainGroup.Jobs.Add(item);
        }

        JobGroups.Add(mainGroup);

        var subGroups = items
            .Where(x => x.Job.AssignedToUserId != null && x.Job.AssignedToUserId != currentAccountId)
            .GroupBy(x => x.Job.AssignedToUserId!.Value)
            .OrderBy(g => g.First().DriverName);

        foreach (var groupItems in subGroups)
        {
            var first = groupItems.First();
            var title = string.IsNullOrWhiteSpace(first.DriverName)
                ? "Assigned Driver"
                : first.DriverName;

            var group = new JobGroupViewModel(title);

            foreach (var item in groupItems.OrderBy(x => x.Job.SortOrder))
                group.Jobs.Add(item);

            JobGroups.Add(group);
        }
    }

    private void RefreshFeatureBindings()
    {
        OnPropertyChanged(nameof(IsJobsVisible));
        OnPropertyChanged(nameof(IsJobsEnabled));
        OnPropertyChanged(nameof(IsAddJobVisible));
        OnPropertyChanged(nameof(IsAddJobEnabled));
        OnPropertyChanged(nameof(IsDeliverySignatureVisible));
        OnPropertyChanged(nameof(IsDeliverySignatureEnabled));
    }

    private async Task NavigateToFeatureAsync(AppFeature feature, string route)
    {
        if (!await EnsureFeatureEnabledAsync(feature))
            return;

        await Shell.Current.GoToAsync(route);
    }

    #endregion
}