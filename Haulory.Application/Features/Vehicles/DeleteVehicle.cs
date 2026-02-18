using Haulory.Application.Interfaces.Repositories;

namespace Haulory.Application.Features.Vehicles;

public class DeleteVehicle
{
    #region Dependencies

    private readonly IVehicleAssetRepository _repo;

    #endregion

    #region Constructor

    public DeleteVehicle(IVehicleAssetRepository repo)
    {
        _repo = repo;
    }

    #endregion

    #region Public API

    public async Task<bool> DeleteAsync(Guid vehicleAssetId, CancellationToken ct = default)
    {
        // Deletes a vehicle asset by Id
        // Assumes repository handles existence validation internally
        await _repo.DeleteAsync(vehicleAssetId);

        return true;
    }

    #endregion
}
