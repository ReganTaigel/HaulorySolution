namespace Haulory.Contracts.Settings;

public sealed class UpdateDocumentSettingsRequest
{
    public bool GstEnabled { get; set; }
    public decimal GstRatePercent { get; set; }
    public bool FuelSurchargeEnabled { get; set; }
    public decimal FuelSurchargePercent { get; set; }
    public string InvoicePrefix { get; set; } = "INV";
    public string PodPrefix { get; set; } = "POD";
    public int PaymentTermsDays { get; set; }
    public bool ShowDamageNotesOnPod { get; set; }
    public bool ShowWaitTimeOnPod { get; set; }

}