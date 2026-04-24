using Microsoft.EntityFrameworkCore;

namespace HaulitCore.Mobile.Diagnostics;

// EF Core DbContext for managing local crash log storage on the device.
// Uses SQLite as the underlying database.
public class MobileCrashDbContext : DbContext
{
    // Constructor accepting DbContext options (configured in DI).
    public MobileCrashDbContext(DbContextOptions<MobileCrashDbContext> options)
        : base(options)
    {
    }

    // DbSet representing stored crash log entries.
    public DbSet<CrashLog> CrashLogs => Set<CrashLog>();

    // Configures entity schema, constraints, and indexes.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CrashLog>(entity =>
        {
            // Primary key.
            entity.HasKey(x => x.Id);

            // Indexes to optimise common queries (especially sync and filtering).
            entity.HasIndex(x => x.CreatedUtc); // ordering + batching
            entity.HasIndex(x => x.IsSynced);   // sync filtering
            entity.HasIndex(x => x.Severity);   // filtering by severity
            entity.HasIndex(x => x.Source);     // filtering by source/module

            // Required fields with length constraints for storage efficiency.

            entity.Property(x => x.Source)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Severity)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Message)
                .HasMaxLength(1000)
                .IsRequired();

            // Optional metadata fields.

            entity.Property(x => x.ExceptionType)
                .HasMaxLength(300);

            entity.Property(x => x.AccountId)
                .HasMaxLength(100);

            entity.Property(x => x.OwnerId)
                .HasMaxLength(100);

            entity.Property(x => x.PageName)
                .HasMaxLength(100);

            // Device/app context (required for diagnostics).

            entity.Property(x => x.Platform)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.AppVersion)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.AppBuild)
                .HasMaxLength(50)
                .IsRequired();
        });
    }
}