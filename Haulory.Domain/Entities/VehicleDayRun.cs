namespace Haulory.Domain.Entities;

public sealed class VehicleDayRun
{
    public Guid Id { get; set; }
    public Guid OwnerUserId { get; set; }
    public Guid UserId { get; set; }
    public Guid VehicleAssetId { get; set; }

    public int StartOdometerKm { get; set; }
    public int? EndOdometerKm { get; set; }

    public DateTime StartedAtUtc { get; set; }
    public DateTime? FinishedAtUtc { get; set; }

    public string? Notes { get; set; }

    public UserAccount? OwnerUser { get; set; }
    public UserAccount? User { get; set; }
    public VehicleAsset? VehicleAsset { get; set; }
}