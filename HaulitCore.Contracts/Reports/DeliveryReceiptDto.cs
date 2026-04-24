namespace HaulitCore.Contracts.Reports;

public sealed class DeliveryReceiptDto
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }

    public string ReferenceNumber { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;

    public string PickupCompany { get; set; } = string.Empty;
    public string PickupAddress { get; set; } = string.Empty;

    public string DeliveryCompany { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;

    public string LoadDescription { get; set; } = string.Empty;

    public string? ReceiverName { get; set; }
    public DateTime DeliveredAtUtc { get; set; }

    public DateTime DeliveredLocal
    {
        get
        {
            var utc = DeliveredAtUtc.Kind == DateTimeKind.Utc
                ? DeliveredAtUtc
                : DateTime.SpecifyKind(DeliveredAtUtc, DateTimeKind.Utc);

            return utc.ToLocalTime();
        }
    }

    public string RateType { get; set; } = string.Empty;
    public decimal RateValue { get; set; }
    public decimal Quantity { get; set; }

    public decimal Total { get; set; }

    // GST
    public bool GstEnabled { get; set; }
    public decimal GstRatePercent { get; set; }

    // Fuel
    public bool FuelSurchargeEnabled { get; set; }
    public decimal FuelSurchargePercent { get; set; }
    public decimal FuelSurchargeAmount { get; set; }

    // Wait time charges 
    public bool WaitTimeChargeEnabled { get; set; }
    public decimal WaitTimeChargeAmount { get; set; }

    // Hand unload charges  
    public bool HandUnloadChargeEnabled { get; set; }
    public decimal HandUnloadChargeAmount { get; set; }

    // POD / delivery extras
    public string? DamageNotes { get; set; }
    public int? WaitTimeMinutes { get; set; }

    public bool ShowDamageNotesOnPod { get; set; }
    public bool ShowWaitTimeOnPod { get; set; }
}