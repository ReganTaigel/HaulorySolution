using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;

namespace Haulory.Application.Features.Vehicles;

public class UpdateVehicleCompliance
{
    #region Dependencies

    private readonly IVehicleAssetRepository _repo;

    #endregion

    #region Constructor

    public UpdateVehicleCompliance(IVehicleAssetRepository repo)
    {
        _repo = repo;
    }

    #endregion

    #region Public API

    public async Task<bool> UpdateAsync(VehicleAsset updatedAsset, CancellationToken ct = default)
    {
        // Update an existing vehicle asset compliance record
        // Repository is responsible for:
        // - Ensuring asset exists
        // - Handling persistence (SQL / JSON / etc.)
        // - Managing concurrency if required

        await _repo.UpdateAsync(updatedAsset);

        return true;
    }

    #endregion
}
