namespace HaulitCore.Contracts.Settings;

public sealed class UpdateDocumentSettingsRequest
{
    public bool GstEnabled { get; set; }
    public decimal GstRatePercent { get; set; }

    public bool FuelSurchargeEnabled { get; set; }
    public decimal FuelSurchargePercent { get; set; }

    public decimal WaitTimeCharge { get; set; }
    public decimal HandUnloadCharge { get; set; }

    public bool WaitTimeChargeEnabled { get; set; }
    public bool HandUnloadChargeEnabled { get; set; }

    public string InvoicePrefix { get; set; } = "INV";
    public string PodPrefix { get; set; } = "POD";

    public int PaymentTermsDays { get; set; }

    public bool ShowDamageNotesOnPod { get; set; }
    public bool ShowWaitTimeOnPod { get; set; }

    public string? BusinessAddress1 { get; set; }
    public string? BusinessSuburb { get; set; }
    public string? BusinessCity { get; set; }
    public string? BusinessRegion { get; set; }
    public string? BusinessPostcode { get; set; }
    public string? BusinessCountry { get; set; }
    public string? SupplierGstNumber { get; set; }
    public string? SupplierNzbn { get; set; }
    public string? BankAccountNumber { get; set; }

}