using Haulory.Domain.Enums;

namespace Haulory.Domain.Rules;

#region Static Class: Rate Type Rules

// Centralizes billing logic for RateType
public static class RateTypeRules
{
    #region Unit Mapping

    // Returns the default quantity unit for a given rate type
    public static QuantityUnit GetDefaultUnit(RateType rateType) =>
        rateType switch
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

    #endregion

    #region Total Calculation

    // Calculates total based on rate type
    // baseAmount is only used for Percentage billing
    public static decimal CalculateTotal(
        RateType rateType,
        decimal rateValue,
        decimal quantity,
        decimal? baseAmount = null)
    {
        return rateType switch
        {
            RateType.FixedFee => rateValue,

            RateType.Percentage =>
                (baseAmount ?? 0m) * (rateValue / 100m),

            _ => rateValue * quantity
        };
    }

    #endregion
}

#endregion
