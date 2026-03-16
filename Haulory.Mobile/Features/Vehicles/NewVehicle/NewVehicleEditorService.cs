using Haulory.Contracts.Vehicles;
using Haulory.Domain.Enums;
using Haulory.Mobile.Services;

namespace Haulory.Mobile.Features.Vehicles.NewVehicle;

public sealed class NewVehicleEditorService
{
    private readonly VehiclesApiService _vehiclesApiService;
    private readonly NewVehicleRequestMapper _mapper;

    public NewVehicleEditorService(VehiclesApiService vehiclesApiService, NewVehicleRequestMapper mapper)
    {
        _vehiclesApiService = vehiclesApiService;
        _mapper = mapper;
    }

    public async Task<NewVehicleFormState> LoadAsync(Guid vehicleId)
    {
        var vehicle = await _vehiclesApiService.GetVehicleByIdAsync(vehicleId);
        if (vehicle == null)
            throw new Exception("Vehicle could not be loaded.");

        var parsedVehicleType = ParseVehicleType(vehicle.VehicleType);
        if (parsedVehicleType == null)
            throw new Exception("Vehicle type could not be determined.");

        var state = new NewVehicleFormState
        {
            EditingVehicleId = vehicleId,
            VehicleType = parsedVehicleType.Value,
            FuelType = ParseEnum<FuelType>(vehicle.FuelType)
        };

        if (!string.IsNullOrWhiteSpace(vehicle.Configuration))
        {
            state.LightConfiguration = ParseEnum<VehicleConfiguration>(vehicle.Configuration);
            state.HeavyConfiguration = ParseEnum<VehicleConfiguration>(vehicle.Configuration);
            state.PowerUnitBodyType = ParseEnum<PowerUnitBodyType>(vehicle.Configuration);
        }

        ApplyUnit(vehicle, state);
        return state;
    }

    public async Task SaveAsync(NewVehicleFormState state)
    {
        var result = await _vehiclesApiService.CreateVehicleSetAsync(_mapper.MapCreate(state));
        if (result.AssetsCreated <= 0)
            throw new Exception("Vehicle set was not created.");
    }

    public async Task UpdateAsync(Guid vehicleId, NewVehicleFormState state)
    {
        await _vehiclesApiService.UpdateVehicleAsync(vehicleId, _mapper.MapUpdate(state));
    }

    private static void ApplyUnit(VehicleDto vehicle, NewVehicleFormState state)
    {
        if (vehicle.UnitNumber == 1 || vehicle.Kind?.Equals("PowerUnit", StringComparison.OrdinalIgnoreCase) == true)
        {
            state.Unit1Rego = vehicle.Rego ?? string.Empty;
            state.Unit1RegoExpiry = vehicle.RegoExpiry;
            state.Unit1Make = vehicle.Make ?? string.Empty;
            state.Unit1Model = vehicle.Model ?? string.Empty;
            state.Unit1Year = vehicle.Year;
            state.Unit1CertExpiry = vehicle.CertificateExpiry;
            state.PowerUnitOdometerKm = vehicle.OdometerKm;
            state.Unit1RucPurchasedDate = vehicle.RucPurchasedDate;
            state.Unit1RucDistancePurchasedKm = vehicle.RucDistancePurchasedKm;
            state.Unit1RucLicenceStartKm = vehicle.RucLicenceStartKm;
            state.Unit1RucLicenceEndKm = vehicle.RucLicenceEndKm;
            return;
        }

        if (vehicle.UnitNumber == 2)
        {
            state.Unit2Rego = vehicle.Rego ?? string.Empty;
            state.Unit2RegoExpiry = vehicle.RegoExpiry;
            state.Unit2Make = vehicle.Make ?? string.Empty;
            state.Unit2Model = vehicle.Model ?? string.Empty;
            state.Unit2Year = vehicle.Year;
            state.Unit2CertExpiry = vehicle.CertificateExpiry;
            state.Trailer1OdometerKm = vehicle.OdometerKm;
            state.Unit2RucPurchasedDate = vehicle.RucPurchasedDate;
            state.Unit2RucDistancePurchasedKm = vehicle.RucDistancePurchasedKm;
            state.Unit2RucLicenceStartKm = vehicle.RucLicenceStartKm;
            state.Unit2RucLicenceEndKm = vehicle.RucLicenceEndKm;
            return;
        }

        if (vehicle.UnitNumber == 3)
        {
            state.Unit3Rego = vehicle.Rego ?? string.Empty;
            state.Unit3RegoExpiry = vehicle.RegoExpiry;
            state.Unit3Make = vehicle.Make ?? string.Empty;
            state.Unit3Model = vehicle.Model ?? string.Empty;
            state.Unit3Year = vehicle.Year;
            state.Unit3CertExpiry = vehicle.CertificateExpiry;
            state.Trailer2OdometerKm = vehicle.OdometerKm;
            state.Unit3RucPurchasedDate = vehicle.RucPurchasedDate;
            state.Unit3RucDistancePurchasedKm = vehicle.RucDistancePurchasedKm;
            state.Unit3RucLicenceStartKm = vehicle.RucLicenceStartKm;
            state.Unit3RucLicenceEndKm = vehicle.RucLicenceEndKm;
        }
    }

    private static VehicleType? ParseVehicleType(string? raw)
        => string.IsNullOrWhiteSpace(raw) ? null : Enum.TryParse<VehicleType>(raw, true, out var parsed) ? parsed : null;

    private static TEnum? ParseEnum<TEnum>(string? raw) where TEnum : struct
        => string.IsNullOrWhiteSpace(raw) ? null : Enum.TryParse<TEnum>(raw, true, out var parsed) ? parsed : null;
}
