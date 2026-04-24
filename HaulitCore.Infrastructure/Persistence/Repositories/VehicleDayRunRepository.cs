using HaulitCore.Application.Interfaces.Repositories;
using HaulitCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HaulitCore.Infrastructure.Persistence.Repositories;

public sealed class VehicleDayRunRepository : IVehicleDayRunRepository
{
    private readonly HaulitCoreDbContext _context;

    public VehicleDayRunRepository(HaulitCoreDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(VehicleDayRun run, CancellationToken cancellationToken = default)
    {
        _context.VehicleDayRuns.Add(run);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<VehicleDayRun?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.VehicleDayRuns
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<VehicleDayRun?> GetLatestByUserAndVehicleAsync(Guid userId, Guid vehicleAssetId, CancellationToken cancellationToken = default)
    {
        return await _context.VehicleDayRuns
            .Where(x => x.UserId == userId && x.VehicleAssetId == vehicleAssetId)
            .OrderByDescending(x => x.StartedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task UpdateAsync(VehicleDayRun run, CancellationToken cancellationToken = default)
    {
        _context.VehicleDayRuns.Update(run);
        await _context.SaveChangesAsync(cancellationToken);
    }
}