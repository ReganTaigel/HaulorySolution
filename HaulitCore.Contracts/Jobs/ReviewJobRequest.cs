namespace HaulitCore.Contracts.Jobs;

public sealed class ReviewJobRequest
{
    public int? WaitTimeMinutes { get; set; }
    public string? DamageNotes { get; set; }
}