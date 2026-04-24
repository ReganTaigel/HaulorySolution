using HaulitCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace HaulitCore.Infrastructure.Persistence;

public class HaulitCoreDbContext : DbContext
{
    public HaulitCoreDbContext(DbContextOptions<HaulitCoreDbContext> options) : base(options)
    {
        ChangeTracker.DeleteOrphansTiming = CascadeTiming.Immediate;
        ChangeTracker.CascadeDeleteTiming = CascadeTiming.Immediate;
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
    public DbSet<HubodometerReading> HubodometerReadings => Set<HubodometerReading>();
    public DbSet<VehicleDayRun> VehicleDayRuns => Set<VehicleDayRun>();
    public DbSet<ServerCrashLog> ServerCrashLogs => Set<ServerCrashLog>();
    public DbSet<DocumentSettings> DocumentSettings => Set<DocumentSettings>();
    public DbSet<Customer> Customers => Set<Customer>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HaulitCoreDbContext).Assembly);
    }
}
