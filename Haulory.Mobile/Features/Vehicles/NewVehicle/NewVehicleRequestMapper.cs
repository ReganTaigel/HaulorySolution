using Haulory.Contracts.Vehicles;
using Haulory.Domain.Enums;

namespace Haulory.Mobile.Features.Vehicles.NewVehicle;

public sealed class NewVehicleRequestMapper
{
    public CreateVehicleSetRequest MapCreate(NewVehicleFormState state)
    {
        var units = new List<CreateVehicleUnitRequest>();

        if (IsPoweredVehicle(state))
        {
            units.Add(new CreateVehicleUnitRequest
            {
                UnitNumber = 1,
                Kind = "PowerUnit",
                VehicleType = state.VehicleType?.ToString() ?? string.Empty,
                Rego = state.Unit1Rego,
                RegoExpiry = state.Unit1RegoExpiry,
                Make = state.Unit1Make,
                Model = state.Unit1Model,
                Year = state.Unit1Year ?? 0,
                FuelType = state.FuelType?.ToString(),
                Configuration = GetPowerUnitConfigurationForApi(state),
                CertificateType = RequiredCertificate(state).ToString(),
                CertificateExpiry = state.Unit1CertExpiry,
                OdometerKm = state.PowerUnitOdometerKm,
                RucPurchasedDate = state.Unit1RucPurchasedDate,
                RucDistancePurchasedKm = state.Unit1RucDistancePurchasedKm,
                RucLicenceStartKm = state.Unit1RucLicenceStartKm,
                RucLicenceEndKm = state.Unit1RucLicenceEndKm
            });
        }
        else
        {
            units.Add(new CreateVehicleUnitRequest
            {
                UnitNumber = 2,
                Kind = "Trailer",
                VehicleType = state.VehicleType?.ToString() ?? string.Empty,
                Rego = state.Unit2Rego,
                RegoExpiry = state.Unit2RegoExpiry,
                Make = state.Unit2Make,
                Model = state.Unit2Model,
                Year = state.Unit2Year ?? 0,
                Configuration = IsHeavyTrailer(state)
                    ? state.HeavyConfiguration?.ToString()
                    : state.LightConfiguration?.ToString(),
                CertificateType = RequiredCertificate(state).ToString(),
                CertificateExpiry = state.Unit2CertExpiry,
                OdometerKm = state.Trailer1OdometerKm,
                RucPurchasedDate = state.Unit2RucPurchasedDate,
                RucDistancePurchasedKm = state.Unit2RucDistancePurchasedKm,
                RucLicenceStartKm = state.Unit2RucLicenceStartKm,
                RucLicenceEndKm = state.Unit2RucLicenceEndKm
            });

            if (IsBTrain(state))
            {
                units.Add(new CreateVehicleUnitRequest
                {
                    UnitNumber = 3,
                    Kind = "Trailer",
                    VehicleType = state.VehicleType?.ToString() ?? string.Empty,
                    Rego = state.Unit3Rego,
                    RegoExpiry = state.Unit3RegoExpiry,
                    Make = state.Unit3Make,
                    Model = state.Unit3Model,
                    Year = state.Unit3Year ?? 0,
                    Configuration = state.HeavyConfiguration?.ToString(),
                    CertificateType = RequiredCertificate(state).ToString(),
                    CertificateExpiry = state.Unit3CertExpiry,
                    OdometerKm = state.Trailer2OdometerKm,
                    RucPurchasedDate = state.Unit3RucPurchasedDate,
                    RucDistancePurchasedKm = state.Unit3RucDistancePurchasedKm,
                    RucLicenceStartKm = state.Unit3RucLicenceStartKm,
                    RucLicenceEndKm = state.Unit3RucLicenceEndKm
                });
            }
        }

        return new CreateVehicleSetRequest { Units = units };
    }

    public UpdateVehicleRequest MapUpdate(NewVehicleFormState state)
    {
        if (IsPoweredVehicle(state))
        {
            return new UpdateVehicleRequest
            {
                UnitNumber = 1,
                Kind = "PowerUnit",
                VehicleType = state.VehicleType?.ToString() ?? string.Empty,
                Rego = state.Unit1Rego,
                RegoExpiry = state.Unit1RegoExpiry,
                Make = state.Unit1Make,
                Model = state.Unit1Model,
                Year = state.Unit1Year ?? 0,
                FuelType = state.FuelType?.ToString(),
                Configuration = GetPowerUnitConfigurationForApi(state),
                CertificateType = RequiredCertificate(state).ToString(),
                CertificateExpiry = state.Unit1CertExpiry,
                OdometerKm = state.PowerUnitOdometerKm,
                RucPurchasedDate = state.Unit1RucPurchasedDate,
                RucDistancePurchasedKm = state.Unit1RucDistancePurchasedKm,
                RucLicenceStartKm = state.Unit1RucLicenceStartKm,
                RucLicenceEndKm = state.Unit1RucLicenceEndKm
            };
        }

        if (!IsBTrain(state))
        {
            return new UpdateVehicleRequest
            {
                UnitNumber = 2,
                Kind = "Trailer",
                VehicleType = state.VehicleType?.ToString() ?? string.Empty,
                Rego = state.Unit2Rego,
                RegoExpiry = state.Unit2RegoExpiry,
                Make = state.Unit2Make,
                Model = state.Unit2Model,
                Year = state.Unit2Year ?? 0,
                Configuration = IsHeavyTrailer(state)
                    ? state.HeavyConfiguration?.ToString()
                    : state.LightConfiguration?.ToString(),
                CertificateType = RequiredCertificate(state).ToString(),
                CertificateExpiry = state.Unit2CertExpiry,
                OdometerKm = state.Trailer1OdometerKm,
                RucPurchasedDate = state.Unit2RucPurchasedDate,
                RucDistancePurchasedKm = state.Unit2RucDistancePurchasedKm,
                RucLicenceStartKm = state.Unit2RucLicenceStartKm,
                RucLicenceEndKm = state.Unit2RucLicenceEndKm
            };
        }

        return new UpdateVehicleRequest
        {
            UnitNumber = 3,
            Kind = "Trailer",
            VehicleType = state.VehicleType?.ToString() ?? string.Empty,
            Rego = state.Unit3Rego,
            RegoExpiry = state.Unit3RegoExpiry,
            Make = state.Unit3Make,
            Model = state.Unit3Model,
            Year = state.Unit3Year ?? 0,
            Configuration = state.HeavyConfiguration?.ToString(),
            CertificateType = RequiredCertificate(state).ToString(),
            CertificateExpiry = state.Unit3CertExpiry,
            OdometerKm = state.Trailer2OdometerKm,
            RucPurchasedDate = state.Unit3RucPurchasedDate,
            RucDistancePurchasedKm = state.Unit3RucDistancePurchasedKm,
            RucLicenceStartKm = state.Unit3RucLicenceStartKm,
            RucLicenceEndKm = state.Unit3RucLicenceEndKm
        };
    }

    private static bool IsPoweredVehicle(NewVehicleFormState state) =>
        state.VehicleType is VehicleType.LightVehicle or VehicleType.LightCommercial or VehicleType.RigidTruckMedium or VehicleType.RigidTruckHeavy or VehicleType.TractorUnit;

    private static bool IsHeavyTrailer(NewVehicleFormState state) => state.VehicleType == VehicleType.TrailerHeavy;

    private static bool IsBTrain(NewVehicleFormState state) =>
        IsHeavyTrailer(state) &&
        state.HeavyConfiguration is VehicleConfiguration.BDblCurtainsider or VehicleConfiguration.BDblFlatDeck or VehicleConfiguration.BDblRefrigerated or VehicleConfiguration.BDblTanker;

    private static ComplianceCertificateType RequiredCertificate(NewVehicleFormState state) =>
        state.VehicleType is VehicleType.RigidTruckMedium or VehicleType.RigidTruckHeavy or VehicleType.TrailerHeavy
            ? ComplianceCertificateType.Cof
            : ComplianceCertificateType.Wof;

    private static string? GetPowerUnitConfigurationForApi(NewVehicleFormState state)
    {
        if (state.VehicleType == VehicleType.TractorUnit)
            return null;

        return state.PowerUnitBodyType switch
        {
            Domain.Enums.PowerUnitBodyType.Curtainsider => "RigidCurtainsider",
            Domain.Enums.PowerUnitBodyType.FlatDeck => "RigidFlatDeck",
            Domain.Enums.PowerUnitBodyType.Refrigerated => "RigidRefrigerated",
            Domain.Enums.PowerUnitBodyType.Tanker => "RigidTanker",
            _ => null
        };
    }
}
