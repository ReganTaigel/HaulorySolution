using Haulory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Haulory.Infrastructure.Persistence;

public class HauloryDbContext : DbContext
{
    public HauloryDbContext(DbContextOptions<HauloryDbContext> options) : base(options)
    {
    }

    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<VehicleAsset> VehicleAssets => Set<VehicleAsset>();
    public DbSet<DeliveryReceipt> DeliveryReceipts => Set<DeliveryReceipt>();
    public DbSet<WorkSite> WorkSites => Set<WorkSite>();
    public DbSet<InductionRequirement> InductionRequirements => Set<InductionRequirement>();
    public DbSet<DriverInduction> DriverInductions => Set<DriverInduction>();
    public DbSet<JobTrailerAssignment> JobTrailerAssignments => Set<JobTrailerAssignment>();
    public DbSet<OdometerReading> OdometerReadings => Set<OdometerReading>();
    public DbSet<VehicleDayRun> VehicleDayRuns => Set<VehicleDayRun>();
    public DbSet<ServerCrashLog> ServerCrashLogs => Set<ServerCrashLog>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HauloryDbContext).Assembly);
    }
}
