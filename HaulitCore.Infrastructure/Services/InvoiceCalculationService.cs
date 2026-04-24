using HaulitCore.Application.Interfaces.Services;

namespace HaulitCore.Infrastructure.Services;

public sealed class InvoiceCalculationService : IInvoiceCalculationService
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
        decimal handUnloadChargeAmount)
    {
        var subtotal = Math.Round(rateValue * quantity, 2, MidpointRounding.AwayFromZero);

        var waitCharge = waitTimeChargeEnabled
            ? Math.Round(waitTimeChargeAmount, 2, MidpointRounding.AwayFromZero)
            : 0m;

        var unloadCharge = handUnloadChargeEnabled
            ? Math.Round(handUnloadChargeAmount, 2, MidpointRounding.AwayFromZero)
            : 0m;

        var fuelSurchargeAmount = fuelSurchargeEnabled
            ? Math.Round(subtotal * (fuelSurchargePercent / 100m), 2, MidpointRounding.AwayFromZero)
            : 0m;

        var taxableAmount = subtotal + waitCharge + unloadCharge + fuelSurchargeAmount;

        var gstAmount = gstEnabled
            ? Math.Round(taxableAmount * (gstRatePercent / 100m), 2, MidpointRounding.AwayFromZero)
            : 0m;

        var total = taxableAmount + gstAmount;

        return new InvoiceCalculationResult
        {
            Subtotal = subtotal,
            FuelSurchargeAmount = fuelSurchargeAmount,
            WaitTimeChargeAmount = waitCharge,
            HandUnloadChargeAmount = unloadCharge,
            GstAmount = gstAmount,
            Total = total
        };
    }
}