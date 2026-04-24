namespace HaulitCore.Application.Hubodometering.Models;

public class AdjustHubodometerRequest
{
    public Guid VehicleAssetId { get; set; }
    public int UnitNumber { get; set; }
    public int CorrectedKm { get; set; }
    public string Reason { get; set; } = string.Empty;
}