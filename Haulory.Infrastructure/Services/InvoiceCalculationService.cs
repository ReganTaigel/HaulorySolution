using Haulory.Application.Interfaces.Services;

namespace Haulory.Infrastructure.Services;

public sealed class InvoiceCalculationService : IInvoiceCalculationService
{
    public InvoiceCalculationResult Calculate(
        decimal rateValue,
        decimal quantity,
        bool gstEnabled,
        decimal gstRatePercent,
        bool fuelSurchargeEnabled,
        decimal fuelSurchargePercent)
    {
        var subtotal = Math.Round(rateValue * quantity, 2, MidpointRounding.AwayFromZero);

        var fuelSurchargeAmount = fuelSurchargeEnabled
            ? Math.Round(subtotal * (fuelSurchargePercent / 100m), 2, MidpointRounding.AwayFromZero)
            : 0m;

        var taxableAmount = subtotal + fuelSurchargeAmount;

        var gstAmount = gstEnabled
            ? Math.Round(taxableAmount * (gstRatePercent / 100m), 2, MidpointRounding.AwayFromZero)
            : 0m;

        var total = taxableAmount + gstAmount;

        return new InvoiceCalculationResult
        {
            Subtotal = subtotal,
            FuelSurchargeAmount = fuelSurchargeAmount,
            GstAmount = gstAmount,
            Total = total
        };
    }
}