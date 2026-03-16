namespace Haulory.Contracts.Vehicles;

public sealed class OdometerReadingResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
