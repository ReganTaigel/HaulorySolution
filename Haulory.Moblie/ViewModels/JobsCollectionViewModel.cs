using System.Collections.ObjectModel;
using System.Windows.Input;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Interfaces.Services;
using Haulory.Domain.Entities;
using Haulory.Mobile.Contracts.Jobs;
using Haulory.Mobile.Services;
using Haulory.Mobile.Views;
using Microsoft.Maui.Controls;

namespace Haulory.Mobile.ViewModels;

public class JobsCollectionViewModel : BaseViewModel
{
    private readonly JobsApiService _jobsApiService;
    private readonly IDriverRepository _driverRepository;
    private readonly IVehicleAssetRepository _vehicleAssetRepository;
    private readonly ISessionService _session;

    public ObservableCollection<JobListItemViewModel> Jobs { get; } = new();
    public ObservableCollection<JobGroupViewModel> JobGroups { get; } = new();

    public ICommand AddJobCommand { get; }
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

    public JobsCollectionViewModel(
        JobsApiService jobsApiService,
        IDriverRepository driverRepository,
        IVehicleAssetRepository vehicleAssetRepository,
        ISessionService session)
    {
        _jobsApiService = jobsApiService;
        _driverRepository = driverRepository;
        _vehicleAssetRepository = vehicleAssetRepository;
        _session = session;

        AddJobCommand = new Command(async () =>
        {
            if (!IsMainUser) return;
            await Shell.Current.GoToAsync(nameof(NewJobPage));
        });

        SignDeliveryCommand = new Command<JobListItemViewModel>(async item =>
        {
            if (item?.Job == null) return;

            await Shell.Current.GoToAsync(
                $"{nameof(DeliverySignaturePage)}?jobId={item.Job.Id}");
        });

        MoveUpCommand = new Command<JobListItemViewModel>(async item =>
        {
            if (!IsMainUser) return;
            await Shell.Current.DisplayAlertAsync(
                "Not moved",
                "Job reordering is still using the local repository flow and has not been moved to the API yet.",
                "OK");
        });

        MoveDownCommand = new Command<JobListItemViewModel>(async item =>
        {
            if (!IsMainUser) return;
            await Shell.Current.DisplayAlertAsync(
                "Not moved",
                "Job reordering is still using the local repository flow and has not been moved to the API yet.",
                "OK");
        });
    }

    public async Task LoadAsync()
    {
        Jobs.Clear();
        JobGroups.Clear();

        var ownerUserId = _session.CurrentOwnerId ?? Guid.Empty;
        var accountId = _session.CurrentAccountId ?? Guid.Empty;

        if (ownerUserId == Guid.Empty || accountId == Guid.Empty)
            return;

        var jobs = (await _jobsApiService.GetActiveJobsAsync()).ToList();

        jobs = jobs.OrderBy(j => j.SortOrder).ToList();

        var driverIds = jobs.Where(j => j.DriverId.HasValue)
            .Select(j => j.DriverId!.Value)
            .Distinct()
            .ToList();

        var vehicleIds = jobs.Where(j => j.VehicleAssetId.HasValue)
            .Select(j => j.VehicleAssetId!.Value)
            .Distinct()
            .ToList();

        var trailerIds = jobs
            .SelectMany(j => j.TrailerAssetIds)
            .Distinct()
            .ToList();

        var driversById = new Dictionary<Guid, Driver>();
        foreach (var id in driverIds)
        {
            var d = await _driverRepository.GetByIdAsync(id);
            if (d != null)
                driversById[id] = d;
        }

        var assetsById = new Dictionary<Guid, VehicleAsset>();
        foreach (var id in vehicleIds.Concat(trailerIds).Distinct())
        {
            var a = await _vehicleAssetRepository.GetByIdAsync(id);
            if (a != null)
                assetsById[id] = a;
        }

        var showPricing = IsMainUser;

        var items = new List<JobListItemViewModel>();

        foreach (var job in jobs)
        {
            var driverName = "—";
            if (job.DriverId is Guid did &&
                driversById.TryGetValue(did, out var d) &&
                d != null)
            {
                driverName = $"{d.FirstName} {d.LastName}".Trim();
            }

            var truck = "—";
            if (job.VehicleAssetId is Guid vid &&
                assetsById.TryGetValue(vid, out var v) &&
                v != null)
            {
                truck = $"{v.Make} {v.Model} • {v.Rego}".Trim();
            }

            items.Add(new JobListItemViewModel(job, driverName, truck, showPricing));
        }

        if (IsSubUser)
        {
            foreach (var item in items)
                Jobs.Add(item);
        }
        else
        {
            BuildMainUserGroups(items, accountId);
        }

        OnPropertyChanged(nameof(IsMainUser));
        OnPropertyChanged(nameof(IsSubUser));
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
            var title = string.IsNullOrWhiteSpace(first.DriverName) ? "Assigned Driver" : first.DriverName;

            var group = new JobGroupViewModel(title);

            foreach (var item in g.OrderBy(x => x.Job.SortOrder))
                group.Jobs.Add(item);

            JobGroups.Add(group);
        }
    }
}