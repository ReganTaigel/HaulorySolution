using System.Collections.ObjectModel;
using System.Windows.Input;
using Haulory.Application.Interfaces.Services;
using Haulory.Contracts.Drivers;
using Haulory.Contracts.Jobs;
using Haulory.Contracts.Vehicles;
using Haulory.Mobile.Features;
using Haulory.Mobile.Services;
using Haulory.Mobile.Views;
using Microsoft.Maui.Controls;

namespace Haulory.Mobile.ViewModels;

public class JobsCollectionViewModel : BaseViewModel
{
    private readonly JobsApiService _jobsApiService;
    private readonly DriversApiService _driversApiService;
    private readonly VehiclesApiService _vehiclesApiService;
    private readonly ISessionService _session;

    public ObservableCollection<JobListItemViewModel> Jobs { get; } = new();
    public ObservableCollection<JobGroupViewModel> JobGroups { get; } = new();

    public ICommand AddJobCommand { get; }
    public ICommand EditJobCommand { get; }
    public ICommand MoveUpCommand { get; }
    public ICommand MoveDownCommand { get; }
    public ICommand SignDeliveryCommand { get; }

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

    public JobsCollectionViewModel(
        JobsApiService jobsApiService,
        DriversApiService driversApiService,
        VehiclesApiService vehiclesApiService,
        ISessionService session,
        IFeatureAccessService featureAccessService)
        : base(featureAccessService)
    {
        _jobsApiService = jobsApiService;
        _driversApiService = driversApiService;
        _vehiclesApiService = vehiclesApiService;
        _session = session;

        AddJobCommand = new Command(async () =>
        {
            if (!IsMainUser)
                return;

            await NavigateToFeatureAsync(AppFeature.AddJob, nameof(NewJobPage));
        });

        EditJobCommand = new Command<JobListItemViewModel>(async item =>
        {
            if (item?.Job == null)
                return;

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
        });

        SignDeliveryCommand = new Command<JobListItemViewModel>(async item =>
        {
            if (item?.Job == null)
                return;

            if (!await EnsureFeatureEnabledAsync(AppFeature.DeliverySignature))
                return;

            await Shell.Current.GoToAsync(
                $"{nameof(DeliverySignaturePage)}?jobId={item.Job.Id}");
        });

        MoveUpCommand = new Command<JobListItemViewModel>(async item =>
        {
            if (!IsMainUser)
                return;

            if (!await EnsureFeatureEnabledAsync(AppFeature.Jobs))
                return;

            await Shell.Current.DisplayAlertAsync(
                "Not moved",
                "Job reordering has not been moved to the API yet.",
                "OK");
        });

        MoveDownCommand = new Command<JobListItemViewModel>(async item =>
        {
            if (!IsMainUser)
                return;

            if (!await EnsureFeatureEnabledAsync(AppFeature.Jobs))
                return;

            await Shell.Current.DisplayAlertAsync(
                "Not moved",
                "Job reordering has not been moved to the API yet.",
                "OK");
        });
    }

    public async Task LoadAsync()
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
        foreach (var id in driverIds)
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
    }

    private void BuildMainUserGroups(List<JobListItemViewModel> items, Guid currentAccountId)
    {
        var mainGroup = new JobGroupViewModel("Main User", isMainGroup: true);

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

        foreach (var g in subGroups)
        {
            var first = g.First();
            var title = string.IsNullOrWhiteSpace(first.DriverName)
                ? "Assigned Driver"
                : first.DriverName;

            var group = new JobGroupViewModel(title);

            foreach (var item in g.OrderBy(x => x.Job.SortOrder))
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
}