using Haulory.Api.Contracts.Vehicles;
using Haulory.Domain.Entities;

namespace Haulory.Api.Extensions;

public static class VehicleMappingExtensions
{
    public static VehicleDto ToDto(this VehicleAsset vehicle)
    {
        return new VehicleDto
        {
            Id = vehicle.Id,
            OwnerUserId = vehicle.OwnerUserId,
            VehicleSetId = vehicle.VehicleSetId,

            Rego = vehicle.Rego,
            Make = vehicle.Make,
            Model = vehicle.Model,
            Year = vehicle.Year,

            UnitNumber = vehicle.UnitNumber,

            Kind = vehicle.Kind.ToString(),
            VehicleType = vehicle.VehicleType?.ToString(),

            OdometerKm = vehicle.OdometerKm,

            RegoExpiry = vehicle.RegoExpiry,

            CertificateType = vehicle.CertificateType.ToString(),
            CertificateExpiry = vehicle.CertificateExpiry,

            RucLicenceStartKm = vehicle.RucLicenceStartKm,
            RucLicenceEndKm = vehicle.RucLicenceEndKm,

            CreatedUtc = vehicle.CreatedUtc
        };
    }
}