using Haulory.Contracts.Vehicles;
using Haulory.Domain.Entities;

namespace Haulory.Api.Extensions;

// Provides mapping extensions for converting VehicleAsset domain entities into DTOs.
public static class VehicleMappingsExtensions
{
    // Maps a VehicleAsset entity to a VehicleDto for API responses.
    public static VehicleDto ToDto(this VehicleAsset vehicle)
    {
        return new VehicleDto
        {
            Id = vehicle.Id,
            OwnerUserId = vehicle.OwnerUserId,
            VehicleSetId = vehicle.VehicleSetId,

            // Core vehicle identity details.
            Rego = vehicle.Rego,
            Make = vehicle.Make,
            Model = vehicle.Model,
            Year = vehicle.Year,

            // Unit position within a vehicle set (e.g., truck = 1, trailers = 2/3).
            UnitNumber = vehicle.UnitNumber,

            // Convert enums to string representations for API consumers.
            Kind = vehicle.Kind.ToString(),
            VehicleType = vehicle.VehicleType.ToString(),

            // Optional enum fields (nullable-safe mapping).
            FuelType = vehicle.FuelType?.ToString(),
            Configuration = vehicle.Configuration?.ToString(),

            // Operational data.
            OdometerKm = vehicle.OdometerKm,

            // Registration details.
            RegoExpiry = vehicle.RegoExpiry,

            // Compliance certificate details.
            CertificateType = vehicle.CertificateType.ToString(),
            CertificateExpiry = vehicle.CertificateExpiry,

            // Road User Charges (RUC) tracking.
            RucPurchasedDate = vehicle.RucPurchasedDate,
            RucDistancePurchasedKm = vehicle.RucDistancePurchasedKm,
            RucLicenceStartKm = vehicle.RucLicenceStartKm,
            RucLicenceEndKm = vehicle.RucLicenceEndKm,

            // Audit field for creation timestamp.
            CreatedUtc = vehicle.CreatedUtc
        };
    }
}