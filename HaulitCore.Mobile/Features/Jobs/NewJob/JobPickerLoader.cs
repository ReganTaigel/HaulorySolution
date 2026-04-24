using HaulitCore.Domain.Enums;
using HaulitCore.Mobile.Services;
using HaulitCore.Mobile.ViewModels;

namespace HaulitCore.Mobile.Features.Jobs.NewJob;

public sealed class JobPickerLoader
{
    private readonly DriversApiService _driversApiService;
    private readonly VehiclesApiService _vehiclesApiService;

    public JobPickerLoader(
        DriversApiService driversApiService,
        VehiclesApiService vehiclesApiService)
    {
        _driversApiService = driversApiService;
        _vehiclesApiService = vehiclesApiService;
    }

    public async Task<JobPickerData> LoadAsync(CancellationToken cancellationToken = default)
    {
        var drivers = await _driversApiService.GetDriversAsync(cancellationToken);
        var vehicles = await _vehiclesApiService.GetVehiclesAsync(cancellationToken);
        var trailers = await _vehiclesApiService.GetTrailersAsync(cancellationToken);

        return new JobPickerData(
            drivers
                .OrderBy(d => d.LastName)
                .ThenBy(d => d.FirstName)
                .Select(d => new DriverPickerItem
                {
                    Id = d.Id,
                    UserId = d.UserId,
                    DisplayName = $"{d.FirstName} {d.LastName}".Trim()
                })
                .ToList(),
            vehicles
                .Where(v => ParseAssetKind(v.Kind) == AssetKind.PowerUnit)
                .OrderBy(v => v.Rego)
                .Select(v => new VehiclePickerItem
                {
                    Id = v.Id,
                    DisplayName = $"{v.UnitNumber} - {v.Rego} - {v.Make} {v.Model}"
                })
                .ToList(),
            trailers
                .Where(t => ParseAssetKind(t.Kind) == AssetKind.Trailer)
                .OrderBy(t => t.Rego)
                .Select(t => new VehiclePickerItem
                {
                    Id = t.Id,
                    DisplayName = $"{t.UnitNumber} - {t.Rego} - {t.Make} {t.Model}"
                })
                .ToList());
    }

    private static AssetKind? ParseAssetKind(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        if (Enum.TryParse<AssetKind>(raw, true, out var parsed))
            return parsed;

        if (int.TryParse(raw, out var numeric) &&
            Enum.IsDefined(typeof(AssetKind), numeric))
            return (AssetKind)numeric;

        return null;
    }
}

public sealed record JobPickerData(
    IReadOnlyList<DriverPickerItem> Drivers,
    IReadOnlyList<VehiclePickerItem> Vehicles,
    IReadOnlyList<VehiclePickerItem> Trailers);