namespace Haulory.Domain.Entities;

public class DocumentSettings
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OwnerUserId { get; private set; }

    public bool GstEnabled { get; private set; } = true;
    public decimal GstRatePercent { get; private set; } = 15m;

    public bool FuelSurchargeEnabled { get; private set; } = false;
    public decimal FuelSurchargePercent { get; private set; } = 0m;

    public string InvoicePrefix { get; private set; } = "INV";
    public string PodPrefix { get; private set; } = "POD";

    public int PaymentTermsDays { get; private set; } = 7;

    public bool ShowDamageNotesOnPod { get; private set; } = true;
    public bool ShowWaitTimeOnPod { get; private set; } = true;


    private DocumentSettings() { }

    public DocumentSettings(Guid ownerUserId)
    {
        if (ownerUserId == Guid.Empty)
            throw new ArgumentException("Owner user id is required.");

        OwnerUserId = ownerUserId;
    }

    public void Update(
        bool gstEnabled,
        decimal gstRatePercent,
        bool fuelSurchargeEnabled,
        decimal fuelSurchargePercent,
        string invoicePrefix,
        string podPrefix,
        int paymentTermsDays,
        bool showDamageNotesOnPod,
        bool showWaitTimeOnPod)
    {
        GstEnabled = gstEnabled;
        GstRatePercent = gstRatePercent;
        FuelSurchargeEnabled = fuelSurchargeEnabled;
        FuelSurchargePercent = fuelSurchargePercent;
        InvoicePrefix = invoicePrefix?.Trim() ?? "INV";
        PodPrefix = podPrefix?.Trim() ?? "POD";
        PaymentTermsDays = paymentTermsDays;
        ShowDamageNotesOnPod = showDamageNotesOnPod;
        ShowWaitTimeOnPod = showWaitTimeOnPod;
    }
}