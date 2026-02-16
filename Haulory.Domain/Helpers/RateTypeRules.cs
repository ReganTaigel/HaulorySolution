using Haulory.Domain.Enums;

public static class RateTypeRules
{
    public static QuantityUnit GetDefaultUnit(RateType rateType) => rateType switch
    {
        RateType.PerLoad => QuantityUnit.Load,
        RateType.PerPallet => QuantityUnit.Pallet,
        RateType.PerTonne => QuantityUnit.Tonne,
        RateType.PerKm => QuantityUnit.Km,
        RateType.Hourly => QuantityUnit.Hour,
        RateType.FixedFee => QuantityUnit.None,
        RateType.Percentage => QuantityUnit.None,
        _ => QuantityUnit.None
    };

    public static decimal CalculateTotal(RateType rateType, decimal rateValue, decimal quantity, decimal? baseAmount = null)
    {
        return rateType switch
        {
            RateType.FixedFee => rateValue,
            RateType.Percentage => (baseAmount ?? 0m) * (rateValue / 100m),
            _ => rateValue * quantity
        };
    }
}
