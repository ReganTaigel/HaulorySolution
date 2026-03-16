namespace Haulory.Contracts.Jobs;

public sealed class CreateJobResponse
{
    public string Message { get; set; } = string.Empty;
    public Guid JobId { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}