namespace HaulitCore.Application.Interfaces.Services;

public interface IInvoiceCalculationService
{
    public InvoiceCalculationResult Calculate(
          decimal rateValue,
          decimal quantity,
          bool gstEnabled,
          decimal gstRatePercent,
          bool fuelSurchargeEnabled,
          decimal fuelSurchargePercent,
          bool waitTimeChargeEnabled,
          decimal waitTimeChargeAmount,
          bool handUnloadChargeEnabled,
          decimal handUnloadChargeAmount);
}

public sealed class InvoiceCalculationResult
{
    public decimal Subtotal { get; init; }
    public decimal FuelSurchargeAmount { get; init; }
    public decimal WaitTimeChargeAmount { get; init; }
    public decimal HandUnloadChargeAmount { get; init; }
    public decimal GstAmount { get; init; }
    public decimal Total { get; init; }
}