namespace Haulory.Domain.Enums;

#region Enum: Vehicle Configuration (Global)

// Defines the physical configuration of a vehicle or trailer.
// NOTE: Display strings should live in EnumDisplay or localization resources.
public enum VehicleConfiguration
{
    // Light trailers
    SingleAxle = 0,
    TandemAxle = 1,

    // Rigid trucks
    RigidCurtainsider = 2,
    RigidRefrigerated = 3,
    RigidTanker = 4,
    RigidFlatDeck = 5,
    RigidTipper = 6,

    // Tractor / prime mover
    TractorUnit = 7,

    // Semi trailers
    SemiCurtainsider = 8,
    SemiFlatDeck = 9,
    SemiSkeleton = 10,
    SemiRefrigerated = 11,
    SemiTanker = 12,
    SemiTipper = 13,

    // Drawbar / A-frame / rigid trailers
    DrawbarRefrigerated = 14,
    DrawbarCurtainsider = 15,
    DrawbarTanker = 16,
    DrawbarFlatDeck = 17,
    DrawbarTipper = 18,

    // B-double / B-train trailers
    BDblCurtainsider = 19,
    BDblFlatDeck = 20,
    BDblRefrigerated = 21,
    BDblTanker = 22,
    BDblBottomDumper = 23,
    BDblSideTipper = 24,

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
    Tractor = 4,
    Tipper = 5
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