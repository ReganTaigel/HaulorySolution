namespace HaulitCore.Domain.Entities;

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

    public int PaymentTermsDays { get; private set; } = 28;

    public bool ShowDamageNotesOnPod { get; private set; } = false;
    public bool ShowWaitTimeOnPod { get; private set; } = false;

    public decimal WaitTimeCharge { get; private set; }
    public decimal HandUnloadCharge { get; private set; }
    public bool WaitTimeChargeEnabled { get; private set; }
    public bool HandUnloadChargeEnabled { get; private set; }


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
        decimal waitTimeCharge,
        decimal handUnloadCharge,
        bool waitTimeChargeEnabled,
        bool handUnloadChargeEnabled,
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

        WaitTimeCharge = waitTimeCharge;
        HandUnloadCharge = handUnloadCharge;

        WaitTimeChargeEnabled = waitTimeChargeEnabled;
        HandUnloadChargeEnabled = handUnloadChargeEnabled;

        InvoicePrefix = invoicePrefix;
        PodPrefix = podPrefix;
        PaymentTermsDays = paymentTermsDays;
        ShowDamageNotesOnPod = showDamageNotesOnPod;
        ShowWaitTimeOnPod = showWaitTimeOnPod;
    }
}