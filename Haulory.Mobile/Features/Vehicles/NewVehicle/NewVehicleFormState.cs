using Haulory.Domain.Enums;

namespace Haulory.Mobile.Features.Vehicles.NewVehicle;

public sealed class NewVehicleFormState
{
    public Guid? EditingVehicleId { get; set; }
    public bool IsSaving { get; set; }
    public bool IsLoadingExistingVehicle { get; set; }

    public VehicleType? VehicleType { get; set; }
    public VehicleConfiguration? LightConfiguration { get; set; }
    public VehicleConfiguration? HeavyConfiguration { get; set; }
    public PowerUnitBodyType? PowerUnitBodyType { get; set; }
    public FuelType? FuelType { get; set; }

    public string Unit1Rego { get; set; } = string.Empty;
    public string Unit2Rego { get; set; } = string.Empty;
    public string Unit3Rego { get; set; } = string.Empty;

    public DateTime? Unit1RegoExpiry { get; set; }
    public DateTime? Unit2RegoExpiry { get; set; }
    public DateTime? Unit3RegoExpiry { get; set; }

    public int? PowerUnitOdometerKm { get; set; }
    public int? Trailer1OdometerKm { get; set; }
    public int? Trailer2OdometerKm { get; set; }

    public int? Unit1RucLicenceStartKm { get; set; }
    public int? Unit1RucLicenceEndKm { get; set; }
    public int? Unit2RucLicenceStartKm { get; set; }
    public int? Unit2RucLicenceEndKm { get; set; }
    public int? Unit3RucLicenceStartKm { get; set; }
    public int? Unit3RucLicenceEndKm { get; set; }

    public DateTime? Unit1RucPurchasedDate { get; set; }
    public int? Unit1RucDistancePurchasedKm { get; set; }
    public DateTime? Unit2RucPurchasedDate { get; set; }
    public int? Unit2RucDistancePurchasedKm { get; set; }
    public DateTime? Unit3RucPurchasedDate { get; set; }
    public int? Unit3RucDistancePurchasedKm { get; set; }

    public DateTime? Unit1CertExpiry { get; set; }
    public DateTime? Unit2CertExpiry { get; set; }
    public DateTime? Unit3CertExpiry { get; set; }

    public string Unit1Make { get; set; } = string.Empty;
    public string Unit1Model { get; set; } = string.Empty;
    public int? Unit1Year { get; set; }
    public string Unit2Make { get; set; } = string.Empty;
    public string Unit2Model { get; set; } = string.Empty;
    public int? Unit2Year { get; set; }
    public string Unit3Make { get; set; } = string.Empty;
    public string Unit3Model { get; set; } = string.Empty;
    public int? Unit3Year { get; set; }

    public void ResetOdometers()
    {
        PowerUnitOdometerKm = null;
        Trailer1OdometerKm = null;
        Trailer2OdometerKm = null;
    }

    public void ResetUnit1Ruc()
    {
        Unit1RucPurchasedDate = null;
        Unit1RucDistancePurchasedKm = null;
        Unit1RucLicenceStartKm = null;
        Unit1RucLicenceEndKm = null;
    }

    public void ResetUnit2Ruc()
    {
        Unit2RucPurchasedDate = null;
        Unit2RucDistancePurchasedKm = null;
        Unit2RucLicenceStartKm = null;
        Unit2RucLicenceEndKm = null;
    }

    public void ResetUnit3Ruc()
    {
        Unit3RucPurchasedDate = null;
        Unit3RucDistancePurchasedKm = null;
        Unit3RucLicenceStartKm = null;
        Unit3RucLicenceEndKm = null;
    }

    public void ResetAllRuc()
    {
        ResetUnit1Ruc();
        ResetUnit2Ruc();
        ResetUnit3Ruc();
    }
}
