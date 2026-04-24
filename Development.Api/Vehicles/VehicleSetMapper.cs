using HaulitCore.Contracts.Vehicles;
using HaulitCore.Domain.Entities;
using HaulitCore.Domain.Enums;

namespace HaulitCore.Api.Vehicles;

// Maps vehicle set request data into VehicleAsset domain entities.
public sealed class VehicleSetMapper
{
    // Converts a CreateVehicleSetRequest into a list of VehicleAsset entities.
    public List<VehicleAsset> MapAssets(Guid ownerUserId, Guid setId, CreateVehicleSetRequest request)
    {
        var assets = new List<VehicleAsset>();

        // Process units in unit number order to keep output consistent.
        foreach (var unit in request.Units.OrderBy(x => x.UnitNumber))
        {
            // Parse and validate the asset kind.
            if (!Enum.TryParse<AssetKind>(unit.Kind, true, out var kind))
                throw new InvalidOperationException($"Invalid kind for unit {unit.UnitNumber}: {unit.Kind}");

            // Parse and validate the vehicle type.
            if (!Enum.TryParse<VehicleType>(unit.VehicleType, true, out var vehicleType))
                throw new InvalidOperationException($"Invalid vehicle type for unit {unit.UnitNumber}: {unit.VehicleType}");

            // Parse and validate the compliance certificate type.
            if (!Enum.TryParse<ComplianceCertificateType>(unit.CertificateType, true, out var certificateType))
                throw new InvalidOperationException($"Invalid certificate type for unit {unit.UnitNumber}: {unit.CertificateType}");

            // Parse optional fuel type if provided.
            FuelType? fuelType = null;
            if (!string.IsNullOrWhiteSpace(unit.FuelType))
            {
                if (!Enum.TryParse<FuelType>(unit.FuelType, true, out var parsedFuel))
                    throw new InvalidOperationException($"Invalid fuel type for unit {unit.UnitNumber}: {unit.FuelType}");

                fuelType = parsedFuel;
            }

            // Parse optional vehicle configuration if provided.
            VehicleConfiguration? configuration = null;
            if (!string.IsNullOrWhiteSpace(unit.Configuration))
            {
                if (!Enum.TryParse<VehicleConfiguration>(unit.Configuration, true, out var parsedConfig))
                    throw new InvalidOperationException($"Invalid configuration for unit {unit.UnitNumber}: {unit.Configuration}");

                configuration = parsedConfig;
            }

            // Create a new VehicleAsset entity from the request data.
            assets.Add(new VehicleAsset
            {
                OwnerUserId = ownerUserId,
                VehicleSetId = setId,
                UnitNumber = unit.UnitNumber,
                Kind = kind,
                VehicleType = vehicleType,
                FuelType = fuelType,
                Configuration = configuration,

                // Normalise key string fields before storing.
                Rego = unit.Rego.Trim().ToUpperInvariant(),
                RegoExpiry = unit.RegoExpiry,
                Make = unit.Make.Trim(),
                Model = unit.Model.Trim(),

                Year = unit.Year,
                CertificateType = certificateType,
                CertificateExpiry = unit.CertificateExpiry,
                HubodometerKm = unit.HubodometerKm,
                RucPurchasedDate = unit.RucPurchasedDate,
                RucDistancePurchasedKm = unit.RucDistancePurchasedKm,
                RucLicenceStartKm = unit.RucLicenceStartKm,
                RucLicenceEndKm = unit.RucLicenceEndKm,

                // Initialise the next due Hubodometer from the current RUC licence end reading.
                RucNextDueHubodometerKm = unit.RucLicenceEndKm
            });
        }

        return assets;
    }
}