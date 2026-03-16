using Haulory.Contracts.Vehicles;

namespace Haulory.Api.Vehicles;

public sealed class VehicleSetRequestValidator
{
    public string? Validate(CreateVehicleSetRequest request)
    {
        if (request.Units == null || request.Units.Count == 0)
            return "At least one unit is required.";

        foreach (var unit in request.Units.OrderBy(x => x.UnitNumber))
        {
            if (unit.UnitNumber < 1 || unit.UnitNumber > 3)
                return $"Invalid unit number: {unit.UnitNumber}";

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

            if (unit.Year <= 0)
                return $"Year is required for unit {unit.UnitNumber}.";

            if (string.IsNullOrWhiteSpace(unit.CertificateType))
                return $"Certificate type is required for unit {unit.UnitNumber}.";
        }

        return null;
    }
}
