namespace HaulitCore.Contracts.Vehicles;

public sealed class HubodometerReadingResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
