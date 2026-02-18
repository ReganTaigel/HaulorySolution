using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;
using Haulory.Mobile.Views;
using Microsoft.Maui.Controls;

namespace Haulory.Mobile.ViewModels;

public class JobsCollectionViewModel : BaseViewModel
{
    #region Dependencies

    private readonly IJobRepository _jobRepository;
    private readonly IDriverRepository _driverRepository;
    private readonly IVehicleAssetRepository _vehicleAssetRepository;

    #endregion

    #region Collections

    public ObservableCollection<JobListItemViewModel> Jobs { get; } = new();

    #endregion

    #region Commands

    public ICommand AddJobCommand { get; }
    public ICommand MoveUpCommand { get; }
    public ICommand MoveDownCommand { get; }
    public ICommand SignDeliveryCommand { get; }

    #endregion

    #region Constructor

    public JobsCollectionViewModel(
        IJobRepository jobRepository,
        IDriverRepository driverRepository,
        IVehicleAssetRepository vehicleAssetRepository)
    {
        _jobRepository = jobRepository;
        _driverRepository = driverRepository;
        _vehicleAssetRepository = vehicleAssetRepository;

        AddJobCommand = new Command(async () =>
        {
            await Shell.Current.GoToAsync(nameof(NewJobPage));
        });

        SignDeliveryCommand = new Command<JobListItemViewModel>(async item =>
        {
            if (item?.Job == null)
                return;

            await Shell.Current.GoToAsync(
                $"{nameof(DeliverySignaturePage)}?jobId={item.Job.Id}");
        });

        MoveUpCommand = new Command<JobListItemViewModel>(async item =>
            await MoveAsync(item, -1));

        MoveDownCommand = new Command<JobListItemViewModel>(async item =>
            await MoveAsync(item, +1));
    }

    #endregion

    #region Load

    public async Task LoadAsync()
    {
        Jobs.Clear();

        var jobs = (await _jobRepository.GetAllAsync())
            .OrderBy(j => j.SortOrder)
            .ToList();

        // Collect IDs we need to resolve
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

        // Resolve drivers per-id (bulk method can be added later)
        var driversById = new Dictionary<Guid, Driver>();
        foreach (var id in driverIds)
        {
            var d = await _driverRepository.GetByIdAsync(id);
            if (d != null)
                driversById[id] = d;
        }

        // Resolve vehicles per-id (bulk method can be added later)
        var vehiclesById = new Dictionary<Guid, VehicleAsset>();
        foreach (var id in vehicleIds)
        {
            var v = await _vehicleAssetRepository.GetByIdAsync(id);
            if (v != null)
                vehiclesById[id] = v;
        }

        // Build UI list items
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
                vehiclesById.TryGetValue(vid, out var v) &&
                v != null)
            {
                truck = $"{v.Make} {v.Model} • {v.Rego}".Trim();
            }

            Jobs.Add(new JobListItemViewModel(job, driverName, truck));
        }
    }

    #endregion

    #region Reordering

    private async Task MoveAsync(JobListItemViewModel? item, int direction)
    {
        if (item?.Job == null)
            return;

        var list = Jobs.ToList();

        var index = list.FindIndex(x => x.Job.Id == item.Job.Id);
        if (index < 0)
            return;

        var newIndex = index + direction;
        if (newIndex < 0 || newIndex >= list.Count)
            return;

        // Swap
        (list[index], list[newIndex]) = (list[newIndex], list[index]);

        // Re-assign SortOrder sequentially on domain jobs
        for (int i = 0; i < list.Count; i++)
            list[i].Job.SetSortOrder(i + 1);

        // Persist the updated ordering
        await _jobRepository.UpdateAllAsync(list.Select(x => x.Job).ToList());

        // Reload UI collection (preserves driver/truck display)
        Jobs.Clear();
        foreach (var x in list.OrderBy(x => x.Job.SortOrder))
            Jobs.Add(x);
    }

    #endregion
}
