using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Interfaces.Services;
using Haulory.Domain.Entities;
using Haulory.Mobile.Views;
using Microsoft.Maui.Controls;

namespace Haulory.Mobile.ViewModels;

public class JobsCollectionViewModel : BaseViewModel
{
    private readonly IJobRepository _jobRepository;
    private readonly IDriverRepository _driverRepository;
    private readonly IVehicleAssetRepository _vehicleAssetRepository;
    private readonly ISessionService _session;

    public ObservableCollection<JobListItemViewModel> Jobs { get; } = new();

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
        IJobRepository jobRepository,
        IDriverRepository driverRepository,
        IVehicleAssetRepository vehicleAssetRepository,
        ISessionService session)
    {
        _jobRepository = jobRepository;
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
            await MoveAsync(item, -1);
        });

        MoveDownCommand = new Command<JobListItemViewModel>(async item =>
        {
            if (!IsMainUser) return;
            await MoveAsync(item, +1);
        });
    }

    public async Task LoadAsync()
    {
        Jobs.Clear();

        var ownerUserId = _session.CurrentOwnerId ?? Guid.Empty;
        var accountId = _session.CurrentAccountId ?? Guid.Empty;

        if (ownerUserId == Guid.Empty || accountId == Guid.Empty)
            return;

        var jobs = IsSubUser
            ? (await _jobRepository.GetActiveAssignedToUserAsync(ownerUserId, accountId)).ToList()
            : (await _jobRepository.GetActiveByOwnerAsync(ownerUserId)).ToList();

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
            .SelectMany(j => j.TrailerAssignments)
            .Select(t => t.TrailerAssetId)
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

            Jobs.Add(new JobListItemViewModel(job, driverName, truck, showPricing));
        }

        OnPropertyChanged(nameof(IsMainUser));
        OnPropertyChanged(nameof(IsSubUser));
    }

    private async Task MoveAsync(JobListItemViewModel? item, int direction)
    {
        if (item?.Job == null) return;

        var ownerUserId = _session.CurrentOwnerId ?? Guid.Empty;
        if (ownerUserId == Guid.Empty)
            return;

        if (item.Job.IsDelivered)
            return;

        var list = Jobs.ToList();
        var index = list.FindIndex(x => x.Job.Id == item.Job.Id);
        if (index < 0) return;

        var newIndex = index + direction;
        if (newIndex < 0 || newIndex >= list.Count) return;

        (list[index], list[newIndex]) = (list[newIndex], list[index]);

        for (int i = 0; i < list.Count; i++)
            list[i].Job.SetSortOrder(i + 1);

        await _jobRepository.UpdateAllAsync(
            ownerUserId,
            list.Select(x => x.Job).ToList());

        Jobs.Clear();
        foreach (var x in list.OrderBy(x => x.Job.SortOrder))
            Jobs.Add(x);
    }
}