namespace Haulory.Domain.Enums;

public enum RateType
{
    PerLoad,
    PerPallet,
    PerTonne,
    PerKm,
    Hourly,
    FixedFee,
    Percentage
}
public enum QuantityUnit
{
    Load,       // 1 job = 1 load
    Pallet,
    Tonne,
    Km,
    Hour,
    None        // for FixedFee, etc.
}