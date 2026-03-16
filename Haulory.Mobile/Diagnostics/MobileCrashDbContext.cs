using Microsoft.EntityFrameworkCore;

namespace Haulory.Mobile.Diagnostics;

public class MobileCrashDbContext : DbContext
{
    public MobileCrashDbContext(DbContextOptions<MobileCrashDbContext> options)
        : base(options)
    {
    }

    public DbSet<CrashLog> CrashLogs => Set<CrashLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CrashLog>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.CreatedUtc);
            entity.HasIndex(x => x.IsSynced);
            entity.HasIndex(x => x.Severity);
            entity.HasIndex(x => x.Source);

            entity.Property(x => x.Source)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Severity)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Message)
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(x => x.ExceptionType)
                .HasMaxLength(300);

            entity.Property(x => x.AccountId)
                .HasMaxLength(100);

            entity.Property(x => x.OwnerId)
                .HasMaxLength(100);

            entity.Property(x => x.PageName)
                .HasMaxLength(100);

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