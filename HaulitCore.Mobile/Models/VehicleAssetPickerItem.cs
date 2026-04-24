using HaulitCore.Domain.Entities;

namespace HaulitCore.Mobile.Models;

public class VehicleAssetPickerItem
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public int? CurrentHubodometerKm { get; set; }

    public static VehicleAssetPickerItem FromEntity(VehicleAsset asset)
    {
        return new VehicleAssetPickerItem
        {
            Id = asset.Id,
            CurrentHubodometerKm = asset.HubodometerKm,
            DisplayName = $"{asset.TitlePrefix} - {asset.Rego} ({asset.HubodometerKm?.ToString("N0") ?? "—"} km)"
        };
    }
}