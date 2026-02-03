using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;

namespace Haulory.Application.Features.Vehicles;

public class UpdateVehicleCompliance
{
    private readonly IVehicleAssetRepository _repo;

    public UpdateVehicleCompliance(IVehicleAssetRepository repo)
    {
        _repo = repo;
    }

    public async Task<bool> UpdateAsync(VehicleAsset updatedAsset, CancellationToken ct = default)
    {
        // If your repo doesn't support Update yet, you'll add it later.
        // For now, you can implement replace-by-id in JSON storage.
        await _repo.UpdateAsync(updatedAsset);
        return true;
    }
}
