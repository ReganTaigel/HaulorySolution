namespace Haulory.Domain.Enums;

#region Enum: Vehicle Configuration

// Defines the physical configuration of a vehicle or trailer
public enum VehicleConfiguration
{
    // Light Trailers (axle-based)
    SingleAxle = 0,
    TandemAxle = 1,

    // Rigid Trucks
    RigidTruckCurtainsider = 2,
    RigidTruckRefrigerator = 3,
    RigidTruckTanker = 4,
    RigidTruckFlatDeck = 5,

    // Tractor Units
    TractorUint = 6,

    // Semi Trailers
    SemiCurtainsider = 7,
    SemiFlatDeck = 8,
    SemiSkeleton = 9,
    SemiRefrigerator = 10,
    SemiTanker = 11,

    // Rigid Trailers
    RefrigeratorTrailer = 12,
    CurtainSiderTrailer = 13,
    TankerTrailer = 14,
    FlatDeckTrailer = 15,

    // B-Train Trailers
    BCurtainSider = 16,
    BFlatDeck = 17,
    BRefigerator = 18,
    BTanker = 19
}

#endregion

#region Enum: Class 4 Power Unit Type

// Sub-type for Class 4 powered vehicles
public enum Class4PowerUnitType
{
    TruckCurtainsider = 0,
    TruckRefrigerator = 1,
    TruckTanker = 2,
    TruckFlatDeck = 3,
    Tractor = 4
}

#endregion

#region Enum: Compliance Certificate Type

// Type of compliance certificate required
public enum ComplianceCertificateType
{
    None = 0,
    Wof = 1,
    Cof = 2
}

#endregion
