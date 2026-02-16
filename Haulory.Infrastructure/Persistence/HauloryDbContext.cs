using Haulory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Haulory.Infrastructure.Persistence;

public class HauloryDbContext : DbContext
{
    public HauloryDbContext(DbContextOptions<HauloryDbContext> options) : base(options) { }

    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<VehicleAsset> VehicleAssets => Set<VehicleAsset>();
    public DbSet<DeliveryReceipt> DeliveryReceipts => Set<DeliveryReceipt>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //User
        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.Email).IsUnique();

            entity.Property(x => x.Email).IsRequired();
            entity.Property(x => x.PasswordHash).IsRequired();

            entity.HasIndex(x => x.ParentMainUserId);

            // Example if Driver has OwnerUserId:
            // modelBuilder.Entity<Driver>()
            //   .HasOne<User>()
            //   .WithMany()
            //   .HasForeignKey(x => x.OwnerUserId);
        });

        // DRIVER
        // DRIVER
        modelBuilder.Entity<Driver>(entity =>
        {
            entity.HasKey(x => x.Id);

            // Owner relationship (required)
            entity.HasOne<UserAccount>()
                  .WithMany()
                  .HasForeignKey(x => x.OwnerUserId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Optional link to actual User account
            entity.HasOne<UserAccount>()
                  .WithMany()
                  .HasForeignKey(x => x.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.OwnsOne(d => d.EmergencyContact);
        });

        modelBuilder.Entity<VehicleAsset>(entity =>
        {
            entity.HasKey(v => v.Id);

            entity.Property(v => v.OwnerUserId).IsRequired();

            entity.HasOne<UserAccount>()
                  .WithMany()
                  .HasForeignKey(v => v.OwnerUserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(v => v.OwnerUserId);

            entity.HasIndex(v => v.VehicleSetId);
            entity.HasIndex(v => v.Rego);

            entity.Property(v => v.Rego).IsRequired();
            entity.Property(v => v.Make).IsRequired();
            entity.Property(v => v.Model).IsRequired();
            entity.Property(v => v.CreatedUtc).IsRequired();
        });

        modelBuilder.Entity<VehicleAsset>()
            .HasIndex(v => new { v.VehicleSetId, v.UnitNumber })
            .IsUnique();

        modelBuilder.Entity<Job>(entity =>
        {
            entity.Property(j => j.OwnerUserId).IsRequired();
            entity.HasIndex(j => j.OwnerUserId);

            entity.Property(j => j.Quantity).HasColumnType("decimal(18,2)");
            entity.Property(j => j.RateValue).HasColumnType("decimal(18,2)");

            entity.Property(j => j.QuantityUnit).IsRequired();

            entity.HasIndex(j => j.DriverId);
            entity.HasIndex(j => j.VehicleAssetId);

            // Optional FKs (recommended)
            entity.HasOne<UserAccount>()
                  .WithMany()
                  .HasForeignKey(j => j.OwnerUserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Driver>()
                  .WithMany()
                  .HasForeignKey(j => j.DriverId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne<VehicleAsset>()
                  .WithMany()
                  .HasForeignKey(j => j.VehicleAssetId)
                  .OnDelete(DeleteBehavior.SetNull);
            // Optional: if invoice numbers should be unique, uncomment:
            // entity.HasIndex(j => j.InvoiceNumber).IsUnique();
        });

        modelBuilder.Entity<DeliveryReceipt>(entity =>
        {
            entity.HasKey(r => r.Id);

            entity.HasIndex(r => r.JobId).IsUnique(); // 1 receipt per job

            entity.HasOne<Job>()
                  .WithMany()
                  .HasForeignKey(r => r.JobId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(r => r.RateValue).HasColumnType("decimal(18,2)");
            entity.Property(r => r.Total).HasColumnType("decimal(18,2)");

            entity.Property(r => r.SignatureJson).IsRequired();
            entity.Property(r => r.ReceiverName).IsRequired();

            entity.HasIndex(r => r.DeliveredAtUtc);
        });

    }
}
