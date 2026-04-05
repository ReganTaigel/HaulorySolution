using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Limits;
using Haulory.Domain.Entities;
using Haulory.Domain.Enums;

namespace Haulory.Application.Features.Vehicles.CreateVehicleSet;

// Handles the application use case for creating a vehicle set (one or more assets).
// Validates ownership, enforces business rules and plan limits, and persists assets as a group.
public class CreateVehicleHandler
{
    // Repository for vehicle asset persistence and queries.
    private readonly IVehicleAssetRepository _repo;

    // Repository for validating user/tenant ownership.
    private readonly IUserAccountRepository _users;

    // Constructor injection of dependencies.
    public CreateVehicleHandler(
        IVehicleAssetRepository repo,
        IUserAccountRepository users)
    {
        _repo = repo;
        _users = users;
    }

    // Creates a new vehicle set from the supplied command.
    public async Task<CreateVehicleResult> HandleAsync(
        CreateVehicleCommand command,
        CancellationToken ct = default)
    {
        // -----------------------------
        // 1) Validate owner (tenant boundary)
        // -----------------------------

        // Owner must be provided.
        if (command.OwnerUserId == Guid.Empty)
            return new CreateVehicleResult { Success = false, Message = "Owner required." };

        // Owner must exist and be a main (tenant root) user.
        var owner = await _users.GetByIdAsync(command.OwnerUserId);
        if (owner == null || owner.Role != UserRole.Main)
            return new CreateVehicleResult { Success = false, Message = "Invalid owner account." };

        // -----------------------------
        // 2) Validate assets
        // -----------------------------

        // At least one asset must be provided.
        if (command.Assets == null || command.Assets.Count == 0)
            return new CreateVehicleResult { Success = false, Message = "No assets provided." };

        // Ensure all assets share the same VehicleSetId.
        var setId = command.Assets[0].VehicleSetId;

        // Generate a new set ID if not provided.
        if (setId == Guid.Empty)
            setId = Guid.NewGuid();

        foreach (var a in command.Assets)
        {
            // Enforce tenant ownership for all assets.
            a.OwnerUserId = command.OwnerUserId;

            // Ensure all assets belong to the same vehicle set.
            if (a.VehicleSetId == Guid.Empty)
                a.VehicleSetId = setId;

            // Normalize string values to ensure consistency.
            a.Rego = (a.Rego ?? string.Empty).Trim().ToUpperInvariant();
            a.Make = (a.Make ?? string.Empty).Trim();
            a.Model = (a.Model ?? string.Empty).Trim();

            // Validate required fields.
            if (string.IsNullOrWhiteSpace(a.Rego))
                return new CreateVehicleResult { Success = false, Message = "Rego is required." };

            if (a.Year <= 0)
                return new CreateVehicleResult { Success = false, Message = "Year is required." };

            // Business rule validation based on asset kind.
            if (a.Kind == AssetKind.PowerUnit)
            {
                // Powered units must not be classified as trailer types.
                if (a.VehicleType == VehicleType.TrailerHeavy || a.VehicleType == VehicleType.TrailerLight)
                    return new CreateVehicleResult { Success = false, Message = "Powered unit cannot be a trailer type." };
            }
            else if (a.Kind == AssetKind.Trailer)
            {
                // Trailer-specific rules can be enforced here if needed.
                // Example: trailers typically do not have a fuel type.
                // a.FuelType = null;
            }
        }

        // -----------------------------
        // 3) Plan limits (batch-aware)
        // -----------------------------

        // Count how many new assets are being added by type.
        var addPowered = command.Assets.Count(a => a.Kind == AssetKind.PowerUnit);
        var addTrailers = command.Assets.Count(a => a.Kind == AssetKind.Trailer);

        // Retrieve current counts from the system.
        var existingPowered = await _repo.CountPoweredUnitsAsync(command.OwnerUserId);
        var existingTrailers = await _repo.CountTrailersAsync(command.OwnerUserId);

        // Enforce powered unit limits.
        if (existingPowered + addPowered > PlanLimits.MaxPoweredUnits)
            return new CreateVehicleResult
            {
                Success = false,
                Message = $"Powered unit limit reached (max {PlanLimits.MaxPoweredUnits})."
            };

        // Enforce trailer limits.
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
            // Ensure registration number is unique within the tenant.
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

        // Save all assets in a single batch operation.
        await _repo.AddRangeAsync(command.Assets);

        // Return success result with metadata.
        return new CreateVehicleResult
        {
            Success = true,
            Message = "Vehicle set saved.",
            VehicleSetId = setId,
            AssetsCreated = command.Assets.Count
        };
    }
}