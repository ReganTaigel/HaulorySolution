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

    public DbSet<WorkSite> WorkSites => Set<WorkSite>();
    public DbSet<InductionRequirement> InductionRequirements => Set<InductionRequirement>();

    public DbSet<DriverInduction> DriverInductions => Set<DriverInduction>();

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

            entity.Property(d => d.PhoneNumber);
            entity.Property(d => d.DateOfBirthUtc);
            entity.Property(d => d.LicenceExpiresOnUtc);

            entity.Property(d => d.Line1);
            entity.Property(d => d.Line2);
            entity.Property(d => d.Suburb);
            entity.Property(d => d.City);
            entity.Property(d => d.Region);
            entity.Property(d => d.Postcode);
            entity.Property(d => d.Country);
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
        modelBuilder.Entity<WorkSite>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CompanyName);
            entity.Property(x => x.OwnerUserId).IsRequired();
            entity.Property(x => x.Name).IsRequired();
            entity.Property(x => x.IsActive).IsRequired();

            entity.HasIndex(x => x.OwnerUserId);
            entity.HasIndex(x => new { x.OwnerUserId, x.IsActive });
            entity.HasIndex(x => new { x.OwnerUserId, x.Name });
        });

        modelBuilder.Entity<InductionRequirement>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.OwnerUserId).IsRequired();
            entity.Property(x => x.WorkSiteId).IsRequired();
            entity.Property(x => x.Title).IsRequired();
            entity.Property(x => x.ValidForDays);
            entity.Property(x => x.IsActive).IsRequired();
            entity.Property(x => x.PpeRequired);

            entity.HasIndex(x => x.OwnerUserId);
            entity.HasIndex(x => x.WorkSiteId);
            entity.HasIndex(x => new { x.OwnerUserId, x.WorkSiteId, x.IsActive });

            entity.HasOne<WorkSite>()
                  .WithMany()
                  .HasForeignKey(x => x.WorkSiteId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<DriverInduction>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.OwnerUserId).IsRequired();
            entity.Property(x => x.DriverId).IsRequired();
            entity.Property(x => x.WorkSiteId).IsRequired();
            entity.Property(x => x.RequirementId).IsRequired();

            entity.HasIndex(x => new { x.OwnerUserId, x.DriverId, x.WorkSiteId, x.RequirementId })
                  .IsUnique();
            entity.Property(x => x.IssueDateUtc).IsRequired();

            entity.HasIndex(x => new { x.OwnerUserId, x.DriverId });

            // Optional relations (recommended)
            entity.HasOne<Driver>()
                  .WithMany()
                  .HasForeignKey(x => x.DriverId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<WorkSite>()
                  .WithMany()
                  .HasForeignKey(x => x.WorkSiteId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<InductionRequirement>()
                  .WithMany()
                  .HasForeignKey(x => x.RequirementId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

    }
}
