using Haulory.Domain.Enums;

namespace Haulory.Mobile.ViewModels;

// Lightweight DTO for displaying completed jobs in reporting screens.
// Keep this UI-friendly (strings formatted for display, pre-calculated totals),
// and avoid adding behaviour/business rules here.
public class ReportJobItem
{
    #region Identity

    // Unique identifier for the job being reported.
    public Guid JobId { get; init; }

    #endregion

    #region References

    // Customer / internal reference number for the job.
    public string ReferenceNumber { get; init; } = string.Empty;

    // Invoice number associated with the job (if applicable).
    public string InvoiceNumber { get; init; } = string.Empty;

    #endregion

    #region Delivery Details

    // Company the delivery was made for.
    public string DeliveryCompany { get; init; } = string.Empty;

    // Delivery address (display-ready).
    public string DeliveryAddress { get; init; } = string.Empty;

    // Name of the person who received the delivery.
    public string ReceiverName { get; init; } = string.Empty;

    // UTC timestamp when the job was delivered/completed.
    public DateTime DeliveredAtUtc { get; init; }

    #endregion

    #region Pricing

    // Determines how the job is charged (e.g. per hour, per load, per km).
    public RateType RateType { get; init; }

    // The numeric rate value (e.g. $/hr, $/km, $/load depending on RateType).
    public decimal RateValue { get; init; }

    // Quantity used to calculate total (e.g. hours, kms, loads).
    public int Quantity { get; init; }

    // Pre-calculated total for display/reporting (typically RateValue * Quantity).
    public decimal Total { get; init; }

    #endregion
}
