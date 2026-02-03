using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;

namespace Haulory.Application.Features.Vehicles.CreateVehicleSet;

public class CreateVehicleHandler
{
    private readonly IVehicleAssetRepository _repo;

    public CreateVehicleHandler(IVehicleAssetRepository repo)
    {
        _repo = repo;
    }

    public async Task<CreateVehicleResult> HandleAsync(CreateVehicleCommand command, CancellationToken ct = default)
    {
        if (command.Assets == null || command.Assets.Count == 0)
            return new CreateVehicleResult { Success = false, Message = "No assets provided." };

        // Ensure they all share the same set id (wizard run)
        var setId = command.Assets[0].VehicleSetId;
        if (setId == Guid.Empty)
            setId = Guid.NewGuid();

        foreach (var a in command.Assets)
        {
            if (a.VehicleSetId == Guid.Empty)
                a.VehicleSetId = setId;

            // Very basic validation
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
