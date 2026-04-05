using Haulory.Contracts.Vehicles;

namespace Haulory.Api.Vehicles;

// Validates CreateVehicleSetRequest before mapping into domain entities.
// Returns a single error message if validation fails, otherwise null.
public sealed class VehicleSetRequestValidator
{
    // Validates the incoming vehicle set request.
    public string? Validate(CreateVehicleSetRequest request)
    {
        // Ensure at least one unit is provided.
        if (request.Units == null || request.Units.Count == 0)
            return "At least one unit is required.";

        // Validate each unit in a deterministic order.
        foreach (var unit in request.Units.OrderBy(x => x.UnitNumber))
        {
            // Ensure unit number is within expected range (e.g., truck + trailers).
            if (unit.UnitNumber < 1 || unit.UnitNumber > 3)
                return $"Invalid unit number: {unit.UnitNumber}";

            // Validate required string fields.
            if (string.IsNullOrWhiteSpace(unit.Kind))
                return $"Kind is required for unit {unit.UnitNumber}.";

            if (string.IsNullOrWhiteSpace(unit.VehicleType))
                return $"Vehicle type is required for unit {unit.UnitNumber}.";

            if (string.IsNullOrWhiteSpace(unit.Rego))
                return $"Rego is required for unit {unit.UnitNumber}.";

            if (string.IsNullOrWhiteSpace(unit.Make))
                return $"Make is required for unit {unit.UnitNumber}.";

            if (string.IsNullOrWhiteSpace(unit.Model))
                return $"Model is required for unit {unit.UnitNumber}.";

            // Validate numeric fields.
            if (unit.Year <= 0)
                return $"Year is required for unit {unit.UnitNumber}.";

            // Validate compliance-related fields.
            if (string.IsNullOrWhiteSpace(unit.CertificateType))
                return $"Certificate type is required for unit {unit.UnitNumber}.";
        }

        // Return null when validation passes.
        return null;
    }
}