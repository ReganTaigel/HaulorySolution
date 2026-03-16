using Haulory.Domain.Entities;

namespace Haulory.Mobile.Models;

public class VehicleAssetPickerItem
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public int? CurrentOdometerKm { get; set; }

    public static VehicleAssetPickerItem FromEntity(VehicleAsset asset)
    {
        return new VehicleAssetPickerItem
        {
            Id = asset.Id,
            CurrentOdometerKm = asset.OdometerKm,
            DisplayName = $"{asset.TitlePrefix} - {asset.Rego} ({asset.OdometerKm?.ToString("N0") ?? "—"} km)"
        };
    }
}