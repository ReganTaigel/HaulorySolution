namespace HaulitCore.Domain.Enums;

#region Enum: Rate Type

// Defines how a job is billed
public enum RateType
{
    PerLoad = 0,
    PerPallet = 1,
    PerTonne = 2,
    PerKm = 3,
    Hourly = 4,
    FixedFee = 5,
    Percentage = 6
}

#endregion

#region Enum: Quantity Unit

// Represents the measurement unit used for billing calculations
public enum QuantityUnit
{
    Load = 0,      // 1 job = 1 load
    Pallet = 1,
    Tonne = 2,
    Km = 3,
    Hour = 4,

    // Used when quantity is irrelevant (e.g., FixedFee)
    None = 5
}

#endregion
