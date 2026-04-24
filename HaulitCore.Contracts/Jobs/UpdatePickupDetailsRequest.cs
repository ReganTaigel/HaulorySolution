namespace HaulitCore.Contracts.Jobs;

public sealed class UpdatePickupDetailsRequest
{
    public int? WaitTimeMinutes { get; set; }
    public string? DamageNotes { get; set; }
}