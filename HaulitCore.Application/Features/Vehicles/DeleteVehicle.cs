using HaulitCore.Application.Interfaces.Repositories;

namespace HaulitCore.Application.Features.Vehicles;

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
        return await _repo.DeleteAsync(vehicleAssetId, ct);
    }

    #endregion
}