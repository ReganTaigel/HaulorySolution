namespace Haulory.Domain.Enums;

#region Enum: Vehicle Configuration (Global)

// Defines the physical configuration of a vehicle or trailer.
// NOTE: Display strings should live in EnumDisplay or localization resources.
public enum VehicleConfiguration
{
    // Light trailers (axle-based)
    SingleAxle = 0,
    TandemAxle = 1,

    // Rigid trucks
    RigidCurtainsider = 2,
    RigidRefrigerated = 3,
    RigidTanker = 4,
    RigidFlatDeck = 5,

    // Tractor / prime mover
    TractorUnit = 6,

    // Semi trailers
    SemiCurtainsider = 7,
    SemiFlatDeck = 8,
    SemiSkeleton = 9,
    SemiRefrigerated = 10,
    SemiTanker = 11,

    // Drawbar / A-frame / rigid trailers
    DrawbarRefrigerated = 12,
    DrawbarCurtainsider = 13,
    DrawbarTanker = 14,
    DrawbarFlatDeck = 15,

    // B-double / B-train trailers
    BDblCurtainsider = 16,
    BDblFlatDeck = 17,
    BDblRefrigerated = 18,
    BDblTanker = 19
}

#endregion

#region Enum: Class 4 Power Unit Type (Legacy / Optional)

// If you still need this for UI, keep it.
// If it was only for NZ "Class 4" wording, we can rename it too.
public enum PowerUnitBodyType
{
    Curtainsider = 0,
    Refrigerated = 1,
    Tanker = 2,
    FlatDeck = 3,
    Tractor = 4
}

#endregion

#region Enum: Compliance Certificate Type (Global-ish)

// Keep for now, but long-term you'll likely expand:
// e.g., AnnualInspection, DOTInspection, Roadworthy, etc.
public enum ComplianceCertificateType
{
    None = 0,

    // NZ / AU terminology
    Wof = 1,
    Cof = 2
}

#endregion