namespace HaulitCore.Domain.Enums;

#region Enum: Vehicle Type (Global)

// Global-first vehicle classification.
// Keep this stable long-term; move country-specific compliance (NZ classes, DOT, etc.)
// into a CompliancePolicy/CountrySettings module, not enum names.
public enum VehicleType
{
    // Light vehicles (cars/vans/utes)
    LightVehicle = 0,
    LightCommercial = 1,

    // Trucks (rigid)
    RigidTruckMedium = 2,
    RigidTruckHeavy = 3,

    // Prime mover / tractor unit
    TractorUnit = 4,

    // Trailers
    TrailerLight = 5,
    TrailerHeavy = 6
}

#endregion