namespace Haulory.Contracts.Jobs;

public sealed class CompleteJobRequest
{
    public string ReceiverName { get; set; } = string.Empty;

    public string SignatureJson { get; set; } = string.Empty;

    public int? WaitTimeMinutes { get; set; }

    public string? DamageNotes { get; set; }
}