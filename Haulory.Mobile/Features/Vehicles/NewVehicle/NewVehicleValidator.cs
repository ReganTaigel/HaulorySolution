using Haulory.Domain.Enums;

namespace Haulory.Mobile.Features.Vehicles.NewVehicle;

public sealed class NewVehicleValidator
{
    public bool CanSave(NewVehicleFormState state)
    {
        if (state.IsSaving) return false;
        if (!HasVehicleType(state)) return false;
        if (!ReadyForRego(state)) return false;
        if (!RegoComplete(state)) return false;
        if (!RegoExpiryComplete(state)) return false;
        if (!MakeModelComplete(state)) return false;
        if (!CertificateComplete(state)) return false;
        if (IsPoweredVehicle(state) && state.FuelType == null) return false;

        if (OdoCount(state) > 0)
        {
            if (!ShowOdometers(state)) return false;
            if (IsPoweredVehicle(state) && state.PowerUnitOdometerKm == null) return false;
            if (IsHeavyTrailer(state) && state.Trailer1OdometerKm == null) return false;
            if (IsBTrain(state) && state.Trailer2OdometerKm == null) return false;
        }

        return RucComplete(state);
    }

    public bool HasVehicleType(NewVehicleFormState state) => state.VehicleType != null;

    public bool IsPoweredVehicle(NewVehicleFormState state) =>
        state.VehicleType == VehicleType.LightVehicle ||
        state.VehicleType == VehicleType.LightCommercial ||
        state.VehicleType == VehicleType.RigidTruckMedium ||
        state.VehicleType == VehicleType.RigidTruckHeavy ||
        state.VehicleType == VehicleType.TractorUnit;

    public bool IsTrailerAsset(NewVehicleFormState state) =>
        state.VehicleType == VehicleType.TrailerLight ||
        state.VehicleType == VehicleType.TrailerHeavy;

    public bool IsLightTrailer(NewVehicleFormState state) => state.VehicleType == VehicleType.TrailerLight;
    public bool IsHeavyTrailer(NewVehicleFormState state) => state.VehicleType == VehicleType.TrailerHeavy;

    public bool IsBTrain(NewVehicleFormState state) =>
        IsHeavyTrailer(state) &&
        state.HeavyConfiguration is VehicleConfiguration.BDblCurtainsider or VehicleConfiguration.BDblFlatDeck or VehicleConfiguration.BDblRefrigerated or VehicleConfiguration.BDblTanker;

    public bool ShowLightTrailerConfig(NewVehicleFormState state) => IsLightTrailer(state);
    public bool ShowPowerUnitBodyType(NewVehicleFormState state) => state.VehicleType is VehicleType.RigidTruckMedium or VehicleType.RigidTruckHeavy;
    public bool ShowHeavyConfig(NewVehicleFormState state) => IsHeavyTrailer(state);

    public bool ReadyForRego(NewVehicleFormState state) =>
        HasVehicleType(state) &&
        (!ShowLightTrailerConfig(state) || state.LightConfiguration != null) &&
        (!ShowPowerUnitBodyType(state) || state.PowerUnitBodyType != null) &&
        (!ShowHeavyConfig(state) || state.HeavyConfiguration != null);

    public bool IsHeavyVehicle(NewVehicleFormState state) =>
        state.VehicleType is VehicleType.RigidTruckMedium or VehicleType.RigidTruckHeavy or VehicleType.TrailerHeavy;

    public ComplianceCertificateType RequiredCertificate(NewVehicleFormState state) =>
        IsHeavyVehicle(state) ? ComplianceCertificateType.Cof : ComplianceCertificateType.Wof;

    public int RegoCount(NewVehicleFormState state)
    {
        if (!HasVehicleType(state)) return 0;
        if (IsPoweredVehicle(state)) return 1;
        return IsBTrain(state) ? 2 : 1;
    }

    public int OdoCount(NewVehicleFormState state)
    {
        if (!HasVehicleType(state)) return 0;
        if (IsPoweredVehicle(state)) return 1;
        if (IsLightTrailer(state)) return 0;
        if (IsHeavyTrailer(state)) return IsBTrain(state) ? 2 : 1;
        return 0;
    }

    public bool RegoComplete(NewVehicleFormState state)
    {
        if (!ReadyForRego(state)) return false;
        if (IsPoweredVehicle(state)) return !string.IsNullOrWhiteSpace(state.Unit1Rego);
        if (string.IsNullOrWhiteSpace(state.Unit2Rego)) return false;
        if (IsBTrain(state) && string.IsNullOrWhiteSpace(state.Unit3Rego)) return false;
        return true;
    }

    public bool ShowRegoExpiry(NewVehicleFormState state) => ReadyForRego(state) && RegoComplete(state);

    public bool RegoExpiryComplete(NewVehicleFormState state)
    {
        if (!ShowRegoExpiry(state)) return false;
        if (IsPoweredVehicle(state)) return state.Unit1RegoExpiry != null;
        if (state.Unit2RegoExpiry == null) return false;
        if (IsBTrain(state) && state.Unit3RegoExpiry == null) return false;
        return true;
    }

    public bool ShowMakeModel(NewVehicleFormState state) => RegoExpiryComplete(state);

    public bool MakeModelComplete(NewVehicleFormState state)
    {
        if (!ShowMakeModel(state)) return false;
        if (IsPoweredVehicle(state))
        {
            return !string.IsNullOrWhiteSpace(state.Unit1Make) &&
                   !string.IsNullOrWhiteSpace(state.Unit1Model) &&
                   state.Unit1Year != null;
        }

        if (string.IsNullOrWhiteSpace(state.Unit2Make) || string.IsNullOrWhiteSpace(state.Unit2Model) || state.Unit2Year == null)
            return false;

        if (IsBTrain(state) && (string.IsNullOrWhiteSpace(state.Unit3Make) || string.IsNullOrWhiteSpace(state.Unit3Model) || state.Unit3Year == null))
            return false;

        return true;
    }

    public bool ShowCertificateStep(NewVehicleFormState state) => MakeModelComplete(state);
    public int CertificateCount(NewVehicleFormState state) => RegoCount(state);
    public bool ShowUnit1Cert(NewVehicleFormState state) => ShowCertificateStep(state) && IsPoweredVehicle(state);
    public bool ShowUnit2Cert(NewVehicleFormState state) => ShowCertificateStep(state) && IsTrailerAsset(state);
    public bool ShowUnit3Cert(NewVehicleFormState state) => ShowCertificateStep(state) && IsBTrain(state);

    public bool CertificateComplete(NewVehicleFormState state)
    {
        if (!ShowCertificateStep(state)) return false;
        if (IsPoweredVehicle(state)) return state.Unit1CertExpiry != null;
        if (state.Unit2CertExpiry == null) return false;
        if (IsBTrain(state) && state.Unit3CertExpiry == null) return false;
        return true;
    }

    public bool ShowFuelType(NewVehicleFormState state) => CertificateComplete(state) && IsPoweredVehicle(state);
    public bool RequiresRuc(NewVehicleFormState state) => state.FuelType is FuelType.Diesel or FuelType.Electric;
    public bool TrailerRequiresRuc(NewVehicleFormState state) => IsHeavyTrailer(state);

    public bool ShowOdometers(NewVehicleFormState state) =>
        CertificateComplete(state) && ((IsPoweredVehicle(state) && state.FuelType != null) || IsHeavyTrailer(state));

    public bool ShowUnit1Rego(NewVehicleFormState state) => ReadyForRego(state) && IsPoweredVehicle(state);
    public bool ShowUnit2Rego(NewVehicleFormState state) => ReadyForRego(state) && IsTrailerAsset(state);
    public bool ShowUnit3Rego(NewVehicleFormState state) => ReadyForRego(state) && IsBTrain(state);
    public bool ShowPowerUnitOdo(NewVehicleFormState state) => ShowOdometers(state) && IsPoweredVehicle(state);
    public bool ShowTrailer1Odo(NewVehicleFormState state) => ShowOdometers(state) && IsHeavyTrailer(state);
    public bool ShowTrailer2Odo(NewVehicleFormState state) => ShowOdometers(state) && IsBTrain(state);

    public int RucCount(NewVehicleFormState state)
    {
        if (!HasVehicleType(state)) return 0;
        if (IsPoweredVehicle(state)) return state.FuelType != null && RequiresRuc(state) ? 1 : 0;
        if (IsHeavyTrailer(state)) return IsBTrain(state) ? 2 : 1;
        return 0;
    }

    public bool ShowRucStep(NewVehicleFormState state) => ShowOdometers(state) && RucCount(state) > 0;
    public bool ShowUnit1Ruc(NewVehicleFormState state) => ShowRucStep(state) && IsPoweredVehicle(state) && RucCount(state) == 1;
    public bool ShowUnit2Ruc(NewVehicleFormState state) => ShowRucStep(state) && !IsPoweredVehicle(state) && IsHeavyTrailer(state);
    public bool ShowUnit3Ruc(NewVehicleFormState state) => ShowRucStep(state) && IsBTrain(state);

    public bool RucComplete(NewVehicleFormState state) =>
        UnitRucComplete(ShowUnit1Ruc(state), state.Unit1RucLicenceStartKm, state.Unit1RucLicenceEndKm) &&
        UnitRucComplete(ShowUnit2Ruc(state), state.Unit2RucLicenceStartKm, state.Unit2RucLicenceEndKm) &&
        UnitRucComplete(ShowUnit3Ruc(state), state.Unit3RucLicenceStartKm, state.Unit3RucLicenceEndKm);

    public string FuelInfoText(NewVehicleFormState state) =>
        state.FuelType == null
            ? string.Empty
            : RequiresRuc(state)
                ? "Distance-based road tax may apply for this fuel type in some regions."
                : "No distance-based road tax is required for this fuel type in most regions.";

    private static bool UnitRucComplete(bool isShown, int? start, int? end) => !isShown || IsValidRange(start, end);
    private static bool IsValidRange(int? start, int? end) => start != null && end != null && start >= 0 && end > start;
}
