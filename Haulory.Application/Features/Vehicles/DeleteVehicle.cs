using Haulory.Application.Interfaces.Repositories;

namespace Haulory.Application.Features.Vehicles;

public class DeleteVehicle
{
    private readonly IVehicleAssetRepository _repo;

    public DeleteVehicle(IVehicleAssetRepository repo)
    {
        _repo = repo;
    }

    public async Task<bool> DeleteAsync(Guid vehicleAssetId, CancellationToken ct = default)
    {
        await _repo.DeleteAsync(vehicleAssetId);
        return true;
    }
}
