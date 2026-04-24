using HaulitCore.Application.Interfaces.Repositories;
using HaulitCore.Contracts.Vehicles;
using HaulitCore.Domain.Entities;
using HaulitCore.Domain.Enums;

namespace HaulitCore.Application.Features.Vehicles.UpdateVehicleSet;

public sealed class UpdateVehicleSetHandler
{
    private readonly IVehicleAssetRepository _repo;
    private readonly IUserAccountRepository _users;

    public UpdateVehicleSetHandler(
        IVehicleAssetRepository repo,
        IUserAccountRepository users)
    {
        _repo = repo;
        _users = users;
    }

    public async Task<UpdateVehicleSetResult> HandleAsync(
        UpdateVehicleSetCommand command,
        CancellationToken ct = default)
    {
        // CHANGED:
        // Validate owner first, same idea as create flow.
        if (command.OwnerUserId == Guid.Empty)
            return new UpdateVehicleSetResult { Success = false, Message = "Owner required." };

        var owner = await _users.GetByIdAsync(command.OwnerUserId);
        if (owner == null || owner.Role != UserRole.Main)
            return new UpdateVehicleSetResult { Success = false, Message = "Invalid owner account." };

        if (command.VehicleAssetId == Guid.Empty)
            return new UpdateVehicleSetResult { Success = false, Message = "Vehicle asset id required." };

        if (command.Request.Units == null || command.Request.Units.Count == 0)
            return new UpdateVehicleSetResult { Success = false, Message = "At least one unit is required." };

        // CHANGED:
        // Resolve the set from the edited asset id.
        var vehicleSetId = await _repo.GetVehicleSetIdByAssetIdAsync(command.VehicleAssetId, ct);
        if (!vehicleSetId.HasValue)
            return new UpdateVehicleSetResult { Success = false, Message = "Vehicle set not found." };

        // CHANGED:
        // Load the full tracked set.
        var existingAssets = (await _repo.GetByVehicleSetIdAsync(vehicleSetId.Value, ct)).ToList();
        if (existingAssets.Count == 0)
            return new UpdateVehicleSetResult { Success = false, Message = "Vehicle set not found." };

        // Tenant boundary check.
        if (existingAssets.Any(x => x.OwnerUserId != command.OwnerUserId))
            return new UpdateVehicleSetResult { Success = false, Message = "Vehicle set not found." };

        // CHANGED:
        // Validate duplicate regos within the incoming request itself.
        var duplicateRego = command.Request.Units
            .Select(x => (x.Rego ?? string.Empty).Trim().ToUpperInvariant())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .GroupBy(x => x)
            .FirstOrDefault(g => g.Count() > 1);

        if (duplicateRego != null)
            return new UpdateVehicleSetResult { Success = false, Message = $"Duplicate rego in request: {duplicateRego.Key}" };

        foreach (var unit in command.Request.Units.OrderBy(x => x.UnitNumber))
        {
            if (unit.UnitNumber < 1 || unit.UnitNumber > 3)
                return new UpdateVehicleSetResult { Success = false, Message = $"Invalid unit number: {unit.UnitNumber}" };

            if (!Enum.TryParse<AssetKind>(unit.Kind, true, out var parsedKind))
                return new UpdateVehicleSetResult { Success = false, Message = $"Invalid kind for unit {unit.UnitNumber}: {unit.Kind}" };

            if (!Enum.TryParse<VehicleType>(unit.VehicleType, true, out var parsedVehicleType))
                return new UpdateVehicleSetResult { Success = false, Message = $"Invalid vehicle type for unit {unit.UnitNumber}: {unit.VehicleType}" };

            if (!Enum.TryParse<ComplianceCertificateType>(unit.CertificateType, true, out var parsedCertificateType))
                return new UpdateVehicleSetResult { Success = false, Message = $"Invalid certificate type for unit {unit.UnitNumber}: {unit.CertificateType}" };

            FuelType? fuelType = null;
            if (!string.IsNullOrWhiteSpace(unit.FuelType))
            {
                if (!Enum.TryParse<FuelType>(unit.FuelType, true, out var parsedFuel))
                    return new UpdateVehicleSetResult { Success = false, Message = $"Invalid fuel type for unit {unit.UnitNumber}: {unit.FuelType}" };

                fuelType = parsedFuel;
            }

            VehicleConfiguration? configuration = null;
            if (!string.IsNullOrWhiteSpace(unit.Configuration))
            {
                if (!Enum.TryParse<VehicleConfiguration>(unit.Configuration, true, out var parsedConfiguration))
                    return new UpdateVehicleSetResult { Success = false, Message = $"Invalid configuration for unit {unit.UnitNumber}: {unit.Configuration}" };

                configuration = parsedConfiguration;
            }

            var normalizedRego = (unit.Rego ?? string.Empty).Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(normalizedRego))
                return new UpdateVehicleSetResult { Success = false, Message = $"Rego is required for unit {unit.UnitNumber}." };

            if (string.IsNullOrWhiteSpace(unit.Make))
                return new UpdateVehicleSetResult { Success = false, Message = $"Make is required for unit {unit.UnitNumber}." };

            if (string.IsNullOrWhiteSpace(unit.Model))
                return new UpdateVehicleSetResult { Success = false, Message = $"Model is required for unit {unit.UnitNumber}." };

            if (unit.Year <= 0)
                return new UpdateVehicleSetResult { Success = false, Message = $"Year is required for unit {unit.UnitNumber}." };

            // CHANGED:
            // Rego uniqueness check while excluding the current asset id when editing.
            var exists = await _repo.RegoExistsAsync(
                command.OwnerUserId,
                normalizedRego,
                unit.Id);

            if (exists)
                return new UpdateVehicleSetResult { Success = false, Message = $"Rego already exists: {normalizedRego}" };

            // CHANGED:
            // Update existing asset in place if it already exists.
            if (unit.Id.HasValue)
            {
                var existing = existingAssets.FirstOrDefault(x => x.Id == unit.Id.Value);
                if (existing == null)
                    return new UpdateVehicleSetResult { Success = false, Message = $"Existing asset not found for unit {unit.UnitNumber}." };

                existing.UnitNumber = unit.UnitNumber;
                existing.Kind = parsedKind;
                existing.VehicleType = parsedVehicleType;
                existing.FuelType = fuelType;
                existing.Configuration = configuration;
                existing.Rego = normalizedRego;
                existing.RegoExpiry = unit.RegoExpiry;
                existing.Make = unit.Make.Trim();
                existing.Model = unit.Model.Trim();
                existing.Year = unit.Year;
                existing.CertificateType = parsedCertificateType;
                existing.CertificateExpiry = unit.CertificateExpiry;
                existing.HubodometerKm = unit.HubodometerKm;
                existing.RucPurchasedDate = unit.RucPurchasedDate;
                existing.RucDistancePurchasedKm = unit.RucDistancePurchasedKm;
                existing.RucLicenceStartKm = unit.RucLicenceStartKm;
                existing.RucLicenceEndKm = unit.RucLicenceEndKm;
                existing.RucNextDueHubodometerKm = unit.RucLicenceEndKm;

                await _repo.UpdateAsync(existing, ct);
            }
            else
            {
                // CHANGED:
                // Add new unit only if no existing id was supplied.
                var newAsset = new VehicleAsset
                {
                    OwnerUserId = command.OwnerUserId,
                    VehicleSetId = vehicleSetId.Value,
                    UnitNumber = unit.UnitNumber,
                    Kind = parsedKind,
                    VehicleType = parsedVehicleType,
                    FuelType = fuelType,
                    Configuration = configuration,
                    Rego = normalizedRego,
                    RegoExpiry = unit.RegoExpiry,
                    Make = unit.Make.Trim(),
                    Model = unit.Model.Trim(),
                    Year = unit.Year,
                    CertificateType = parsedCertificateType,
                    CertificateExpiry = unit.CertificateExpiry,
                    HubodometerKm = unit.HubodometerKm,
                    RucPurchasedDate = unit.RucPurchasedDate,
                    RucDistancePurchasedKm = unit.RucDistancePurchasedKm,
                    RucLicenceStartKm = unit.RucLicenceStartKm,
                    RucLicenceEndKm = unit.RucLicenceEndKm,
                    RucNextDueHubodometerKm = unit.RucLicenceEndKm
                };

                await _repo.AddAsync(newAsset);
            }
        }

        // Delete units that existed before but are no longer present in the edited set.
        var incomingIds = command.Request.Units
            .Where(x => x.Id.HasValue)
            .Select(x => x.Id!.Value)
            .ToHashSet();

        var toDelete = existingAssets
            .Where(x => !incomingIds.Contains(x.Id))
            .ToList();

        // Do NOT delete assets.
        // Just detach them from the vehicle set so they remain reusable.

        foreach (var asset in toDelete)
        {
            asset.VehicleSetId = Guid.Empty; // or null if nullable

            await _repo.UpdateAsync(asset, ct);
        }

        return new UpdateVehicleSetResult
        {
            Success = true,
            Message = "Vehicle set updated."
        };
    }
}