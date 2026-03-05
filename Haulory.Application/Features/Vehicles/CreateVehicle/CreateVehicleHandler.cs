using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Limits;
using Haulory.Domain.Entities;
using Haulory.Domain.Enums;

namespace Haulory.Application.Features.Vehicles.CreateVehicleSet;

public class CreateVehicleHandler
{
    private readonly IVehicleAssetRepository _repo;
    private readonly IUserAccountRepository _users;

    public CreateVehicleHandler(
        IVehicleAssetRepository repo,
        IUserAccountRepository users)
    {
        _repo = repo;
        _users = users;
    }

    public async Task<CreateVehicleResult> HandleAsync(
        CreateVehicleCommand command,
        CancellationToken ct = default)
    {
        // -----------------------------
        // 1) Validate owner (tenant boundary)
        // -----------------------------
        if (command.OwnerUserId == Guid.Empty)
            return new CreateVehicleResult { Success = false, Message = "Owner required." };

        var owner = await _users.GetByIdAsync(command.OwnerUserId);
        if (owner == null || owner.Role != UserRole.Main)
            return new CreateVehicleResult { Success = false, Message = "Invalid owner account." };

        // -----------------------------
        // 2) Validate assets
        // -----------------------------
        if (command.Assets == null || command.Assets.Count == 0)
            return new CreateVehicleResult { Success = false, Message = "No assets provided." };

        // Ensure all assets share the same VehicleSetId
        var setId = command.Assets[0].VehicleSetId;
        if (setId == Guid.Empty)
            setId = Guid.NewGuid();

        foreach (var a in command.Assets)
        {
            // Force ownership + set id (prevents "OwnerUserId required" issues)
            a.OwnerUserId = command.OwnerUserId;

            if (a.VehicleSetId == Guid.Empty)
                a.VehicleSetId = setId;

            // Normalize strings early (optional, but helps validations)
            a.Rego = (a.Rego ?? string.Empty).Trim().ToUpperInvariant();
            a.Make = (a.Make ?? string.Empty).Trim();
            a.Model = (a.Model ?? string.Empty).Trim();

            // Required fields
            if (string.IsNullOrWhiteSpace(a.Rego))
                return new CreateVehicleResult { Success = false, Message = "Rego is required." };

            if (a.Year <= 0)
                return new CreateVehicleResult { Success = false, Message = "Year is required." };

            // Optional sanity checks
            if (a.Kind == AssetKind.PowerUnit)
            {
                // Example: powered unit should not have VehicleType trailer
                if (a.VehicleType == VehicleType.TrailerHeavy || a.VehicleType == VehicleType.TrailerLight)
                    return new CreateVehicleResult { Success = false, Message = "Powered unit cannot be a trailer type." };
            }
            else if (a.Kind == AssetKind.Trailer)
            {
                // Example: trailer should not have fuel type
                // (leave it if you allow future cases)
                // a.FuelType = null;
            }
        }

        // -----------------------------
        // 3) Plan limits (batch-aware)
        // -----------------------------
        var addPowered = command.Assets.Count(a => a.Kind == AssetKind.PowerUnit);
        var addTrailers = command.Assets.Count(a => a.Kind == AssetKind.Trailer);

        var existingPowered = await _repo.CountPoweredUnitsAsync(command.OwnerUserId);
        var existingTrailers = await _repo.CountTrailersAsync(command.OwnerUserId);

        if (existingPowered + addPowered > PlanLimits.MaxPoweredUnits)
            return new CreateVehicleResult
            {
                Success = false,
                Message = $"Powered unit limit reached (max {PlanLimits.MaxPoweredUnits})."
            };

        if (existingTrailers + addTrailers > PlanLimits.MaxTrailers)
            return new CreateVehicleResult
            {
                Success = false,
                Message = $"Trailer limit reached (max {PlanLimits.MaxTrailers})."
            };

        // -----------------------------
        // 4) Optional: rego uniqueness per owner
        // -----------------------------
        foreach (var a in command.Assets)
        {
            // excludeAssetId is useful if you later re-use this handler for "edit"
            var exists = await _repo.RegoExistsAsync(
                ownerUserId: command.OwnerUserId,
                rego: a.Rego,
                excludeAssetId: a.Id == Guid.Empty ? null : a.Id);

            if (exists)
                return new CreateVehicleResult
                {
                    Success = false,
                    Message = $"Rego already exists: {a.Rego}"
                };
        }

        // -----------------------------
        // 5) Persist
        // -----------------------------
        await _repo.AddRangeAsync(command.Assets);

        return new CreateVehicleResult
        {
            Success = true,
            Message = "Vehicle set saved.",
            VehicleSetId = setId,
            AssetsCreated = command.Assets.Count
        };
    }
}