using Haulory.Application.Interfaces.Repositories;
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
        if (command.OwnerUserId == Guid.Empty)
            return new CreateVehicleResult { Success = false, Message = "Owner required." };

        // 🔒 Ensure OwnerUserId belongs to a MAIN account
        var owner = await _users.GetByIdAsync(command.OwnerUserId);
        if (owner == null || owner.Role != UserRole.Main)
            return new CreateVehicleResult { Success = false, Message = "Invalid owner account." };

        if (command.Assets == null || command.Assets.Count == 0)
            return new CreateVehicleResult { Success = false, Message = "No assets provided." };

        // Ensure they all share the same set id (wizard run)
        var setId = command.Assets[0].VehicleSetId;
        if (setId == Guid.Empty)
            setId = Guid.NewGuid();

        foreach (var a in command.Assets)
        {
            // 🔒 Force owner (but DO NOT touch UnitNumber)
            a.OwnerUserId = command.OwnerUserId;

            if (a.VehicleSetId == Guid.Empty)
                a.VehicleSetId = setId;

            // Basic validation
            if (string.IsNullOrWhiteSpace(a.Rego))
                return new CreateVehicleResult { Success = false, Message = "Rego is required." };

            if (a.Year <= 0)
                return new CreateVehicleResult { Success = false, Message = "Year is required." };
        }

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
