using Haulory.Contracts.Vehicles;
using Haulory.Domain.Entities;
using Haulory.Domain.Enums;

namespace Haulory.Api.Vehicles;

public sealed class VehicleSetMapper
{
    public List<VehicleAsset> MapAssets(Guid ownerUserId, Guid setId, CreateVehicleSetRequest request)
    {
        var assets = new List<VehicleAsset>();

        foreach (var unit in request.Units.OrderBy(x => x.UnitNumber))
        {
            if (!Enum.TryParse<AssetKind>(unit.Kind, true, out var kind))
                throw new InvalidOperationException($"Invalid kind for unit {unit.UnitNumber}: {unit.Kind}");

            if (!Enum.TryParse<VehicleType>(unit.VehicleType, true, out var vehicleType))
                throw new InvalidOperationException($"Invalid vehicle type for unit {unit.UnitNumber}: {unit.VehicleType}");

            if (!Enum.TryParse<ComplianceCertificateType>(unit.CertificateType, true, out var certificateType))
                throw new InvalidOperationException($"Invalid certificate type for unit {unit.UnitNumber}: {unit.CertificateType}");

            FuelType? fuelType = null;
            if (!string.IsNullOrWhiteSpace(unit.FuelType))
            {
                if (!Enum.TryParse<FuelType>(unit.FuelType, true, out var parsedFuel))
                    throw new InvalidOperationException($"Invalid fuel type for unit {unit.UnitNumber}: {unit.FuelType}");

                fuelType = parsedFuel;
            }

            VehicleConfiguration? configuration = null;
            if (!string.IsNullOrWhiteSpace(unit.Configuration))
            {
                if (!Enum.TryParse<VehicleConfiguration>(unit.Configuration, true, out var parsedConfig))
                    throw new InvalidOperationException($"Invalid configuration for unit {unit.UnitNumber}: {unit.Configuration}");

                configuration = parsedConfig;
            }

            assets.Add(new VehicleAsset
            {
                OwnerUserId = ownerUserId,
                VehicleSetId = setId,
                UnitNumber = unit.UnitNumber,
                Kind = kind,
                VehicleType = vehicleType,
                FuelType = fuelType,
                Configuration = configuration,
                Rego = unit.Rego.Trim().ToUpperInvariant(),
                RegoExpiry = unit.RegoExpiry,
                Make = unit.Make.Trim(),
                Model = unit.Model.Trim(),
                Year = unit.Year,
                CertificateType = certificateType,
                CertificateExpiry = unit.CertificateExpiry,
                OdometerKm = unit.OdometerKm,
                RucPurchasedDate = unit.RucPurchasedDate,
                RucDistancePurchasedKm = unit.RucDistancePurchasedKm,
                RucLicenceStartKm = unit.RucLicenceStartKm,
                RucLicenceEndKm = unit.RucLicenceEndKm,
                RucNextDueOdometerKm = unit.RucLicenceEndKm
            });
        }

        return assets;
    }
}
