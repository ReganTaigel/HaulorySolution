namespace Haulory.Application.Interfaces.Services;

public interface IInvoiceCalculationService
{
    InvoiceCalculationResult Calculate(
        decimal rateValue,
        decimal quantity,
        bool gstEnabled,
        decimal gstRatePercent,
        bool fuelSurchargeEnabled,
        decimal fuelSurchargePercent);
}

public sealed class InvoiceCalculationResult
{
    public decimal Subtotal { get; init; }
    public decimal FuelSurchargeAmount { get; init; }
    public decimal GstAmount { get; init; }
    public decimal Total { get; init; }
}