namespace Haulory.Application.Odometering.Models;

public class AdjustOdometerRequest
{
    public Guid VehicleAssetId { get; set; }
    public int UnitNumber { get; set; }
    public int CorrectedKm { get; set; }
    public string Reason { get; set; } = string.Empty;
}