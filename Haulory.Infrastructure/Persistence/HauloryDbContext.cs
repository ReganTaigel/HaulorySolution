using Haulory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Haulory.Infrastructure.Persistence;

public class HauloryDbContext : DbContext
{
    #region Constructor

    public HauloryDbContext(DbContextOptions<HauloryDbContext> options) : base(options) { }

    #endregion

    #region DbSets

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

    #endregion

    #region EF Model Configuration

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUserAccounts(modelBuilder);
        ConfigureDrivers(modelBuilder);
        ConfigureVehicleAssets(modelBuilder);
        ConfigureJobs(modelBuilder);
        ConfigureDeliveryReceipts(modelBuilder);
        ConfigureWorkSites(modelBuilder);
        ConfigureInductionRequirements(modelBuilder);
        ConfigureDriverInductions(modelBuilder);
        ConfigureJobTrailerAssignments(modelBuilder);
        ConfigureOdometerReadings(modelBuilder);
    }

    #endregion

    #region Entity Config: UserAccount

    private static void ConfigureUserAccounts(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.Email).IsRequired().HasMaxLength(320);

            entity.Property(x => x.FirstName).IsRequired().HasMaxLength(120);
            entity.Property(x => x.LastName).IsRequired().HasMaxLength(120);

            entity.Property(x => x.PasswordHash).IsRequired().HasMaxLength(512);

            entity.Property(x => x.Role).IsRequired();
            entity.HasIndex(x => x.ParentMainUserId);

            entity.Property(x => x.PhoneNumber).HasMaxLength(50);
            entity.Property(x => x.DateOfBirthUtc);

            entity.Property(x => x.Line1).HasMaxLength(250);
            entity.Property(x => x.Line2).HasMaxLength(250);
            entity.Property(x => x.Suburb).HasMaxLength(120);
            entity.Property(x => x.City).HasMaxLength(120);
            entity.Property(x => x.Region).HasMaxLength(120);
            entity.Property(x => x.Postcode).HasMaxLength(20);
            entity.Property(x => x.Country).HasMaxLength(120);

            entity.Property(x => x.LicenceExpiresOnUtc);

            entity.Property(x => x.BusinessName).HasMaxLength(200);
            entity.Property(x => x.BusinessEmail).HasMaxLength(320);
            entity.Property(x => x.BusinessPhone).HasMaxLength(50);

            entity.Property(x => x.BusinessAddress1).HasMaxLength(250);
            entity.Property(x => x.BusinessAddress2).HasMaxLength(250);
            entity.Property(x => x.BusinessSuburb).HasMaxLength(120);
            entity.Property(x => x.BusinessCity).HasMaxLength(120);
            entity.Property(x => x.BusinessRegion).HasMaxLength(120);
            entity.Property(x => x.BusinessPostcode).HasMaxLength(20);
            entity.Property(x => x.BusinessCountry).HasMaxLength(120);

            entity.Property(x => x.SupplierGstNumber).HasMaxLength(50);
            entity.Property(x => x.SupplierNzbn).HasMaxLength(50);
        });
    }

    #endregion

    #region Entity Config: Driver

    private static void ConfigureDrivers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Driver>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasOne<UserAccount>()
                  .WithMany()
                  .HasForeignKey(x => x.OwnerUserId)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne<UserAccount>()
                  .WithMany()
                  .HasForeignKey(x => x.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.Property(d => d.FirstName).HasMaxLength(120);
            entity.Property(d => d.LastName).HasMaxLength(120);
            entity.Property(d => d.Email).HasMaxLength(320);

            entity.Property(d => d.PhoneNumber).HasMaxLength(50);
            entity.Property(d => d.DateOfBirthUtc);

            entity.Property(d => d.LicenceNumber).HasMaxLength(64);
            entity.Property(d => d.LicenceVersion).HasMaxLength(64);
            entity.Property(d => d.LicenceClassOrEndorsements).HasMaxLength(200);
            entity.Property(d => d.LicenceIssuedOnUtc);
            entity.Property(d => d.LicenceExpiresOnUtc);
            entity.Property(d => d.LicenceConditionsNotes).HasMaxLength(500);

            entity.Property(d => d.Line1).HasMaxLength(250);
            entity.Property(d => d.Line2).HasMaxLength(250);
            entity.Property(d => d.Suburb).HasMaxLength(120);
            entity.Property(d => d.City).HasMaxLength(120);
            entity.Property(d => d.Region).HasMaxLength(120);
            entity.Property(d => d.Postcode).HasMaxLength(20);
            entity.Property(d => d.Country).HasMaxLength(120);

            entity.Property(d => d.Status).IsRequired();

            entity.OwnsOne(d => d.EmergencyContact, owned =>
            {
                owned.Property(x => x.FirstName).HasMaxLength(120);
                owned.Property(x => x.LastName).HasMaxLength(120);
                owned.Property(x => x.Relationship).HasMaxLength(80);
                owned.Property(x => x.Email).HasMaxLength(320);
                owned.Property(x => x.PhoneNumber).HasMaxLength(50);
                owned.Property(x => x.SecondaryPhoneNumber).HasMaxLength(50);
            });
        });
    }

    #endregion

    #region Entity Config: VehicleAsset 

    private static void ConfigureVehicleAssets(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VehicleAsset>(entity =>
        {
            entity.HasKey(v => v.Id);

            entity.Property(v => v.OwnerUserId).IsRequired();

            entity.HasOne<UserAccount>()
                  .WithMany()
                  .HasForeignKey(v => v.OwnerUserId)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasIndex(v => v.OwnerUserId);
            entity.HasIndex(v => v.VehicleSetId);

            entity.Property(v => v.Rego)
                  .IsRequired()
                  .HasMaxLength(32);

            entity.HasIndex(v => new { v.OwnerUserId, v.Rego })
                  .IsUnique();

            entity.Property(v => v.Make)
                  .IsRequired()
                  .HasMaxLength(120);

            entity.Property(v => v.Model)
                  .IsRequired()
                  .HasMaxLength(120);

            entity.Property(v => v.Year)
                  .IsRequired();

            entity.Property(v => v.CreatedUtc)
                  .IsRequired();

            entity.Property(v => v.Kind).IsRequired();
            entity.Property(v => v.VehicleType);
            entity.Property(v => v.FuelType);
            entity.Property(v => v.Configuration);
            entity.Property(v => v.PowerUnitBodyType);

            entity.Property(v => v.RucOdometerAtPurchaseKm);
            entity.Property(v => v.RucDistancePurchasedKm);
            entity.Property(v => v.RucPurchasedDate);
            entity.Property(v => v.RucNextDueOdometerKm);
            entity.Property(v => v.RucLicenceStartKm);
            entity.Property(v => v.RucLicenceEndKm);

            entity.Property(v => v.OdometerKm);
            entity.Property(v => v.RegoExpiry);
            entity.Property(v => v.CertificateExpiry);
        });

        modelBuilder.Entity<VehicleAsset>()
            .HasIndex(v => new { v.VehicleSetId, v.UnitNumber })
            .IsUnique();
    }

    #endregion

    #region Entity Config: Extra Trailer Asset

    private static void ConfigureJobTrailerAssignments(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<JobTrailerAssignment>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.JobId).IsRequired();
            entity.Property(x => x.TrailerAssetId).IsRequired();
            entity.Property(x => x.Position).IsRequired();

            entity.HasIndex(x => new { x.JobId, x.Position }).IsUnique();
            entity.HasIndex(x => new { x.JobId, x.TrailerAssetId }).IsUnique();

            entity.HasIndex(x => x.JobId);
            entity.HasIndex(x => x.TrailerAssetId);

            entity.HasOne<Job>()
                  .WithMany(j => j.TrailerAssignments)
                  .HasForeignKey(x => x.JobId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<VehicleAsset>()
                  .WithMany()
                  .HasForeignKey(x => x.TrailerAssetId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Job>()
            .Navigation(j => j.TrailerAssignments)
            .HasField("_trailerAssignments");
    }

    #endregion

    #region Entity Config: Job

    private static void ConfigureJobs(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(j => j.Id);

            entity.Property(j => j.OwnerUserId).IsRequired();
            entity.HasIndex(j => j.OwnerUserId);

            entity.Property(j => j.SortOrder).IsRequired();
            entity.HasIndex(j => new { j.OwnerUserId, j.SortOrder });

            entity.Property(j => j.ClientCompanyName).IsRequired().HasMaxLength(200);
            entity.Property(j => j.ClientContactName).HasMaxLength(200);
            entity.Property(j => j.ClientEmail).HasMaxLength(320);
            entity.Property(j => j.ClientAddressLine1).IsRequired().HasMaxLength(250);
            entity.Property(j => j.ClientCity).IsRequired().HasMaxLength(120);
            entity.Property(j => j.ClientCountry).IsRequired().HasMaxLength(120);

            entity.Property(j => j.ReferenceNumber).HasMaxLength(64);
            entity.Property(j => j.PickupCompany).HasMaxLength(200);
            entity.Property(j => j.PickupAddress).HasMaxLength(250);
            entity.Property(j => j.DeliveryCompany).HasMaxLength(200);
            entity.Property(j => j.DeliveryAddress).HasMaxLength(250);
            entity.Property(j => j.LoadDescription).HasMaxLength(500);

            entity.Property(j => j.Quantity).HasColumnType("decimal(18,2)");
            entity.Property(j => j.RateValue).HasColumnType("decimal(18,2)");
            entity.Property(j => j.QuantityUnit).IsRequired().HasMaxLength(30);

            entity.Property(j => j.InvoiceNumber).IsRequired().HasMaxLength(64);
            entity.HasIndex(j => new { j.OwnerUserId, j.InvoiceNumber }).IsUnique();

            entity.Property(j => j.DeliverySignatureJson).IsRequired(false);
            entity.Property(j => j.PickupSignatureJson).IsRequired(false);
            entity.Property(j => j.ReceiverName).HasMaxLength(200);
            entity.Property(j => j.PickupSignedByName).HasMaxLength(200);

            entity.HasIndex(j => j.DriverId);
            entity.HasIndex(j => j.VehicleAssetId);

            entity.HasOne<UserAccount>()
                  .WithMany()
                  .HasForeignKey(j => j.OwnerUserId)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne<Driver>()
                  .WithMany()
                  .HasForeignKey(j => j.DriverId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne<VehicleAsset>()
                  .WithMany()
                  .HasForeignKey(j => j.VehicleAssetId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }

    #endregion

    #region Entity Config: DeliveryReceipt

    private static void ConfigureDeliveryReceipts(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DeliveryReceipt>(entity =>
        {
            entity.HasKey(r => r.Id);

            entity.Property(r => r.OwnerUserId).IsRequired();
            entity.HasIndex(r => r.OwnerUserId);

            entity.HasIndex(r => new { r.OwnerUserId, r.JobId }).IsUnique();

            entity.Property(r => r.ReferenceNumber).HasMaxLength(64);
            entity.Property(r => r.InvoiceNumber).HasMaxLength(64);
            entity.Property(r => r.PickupCompany).HasMaxLength(200);
            entity.Property(r => r.PickupAddress).HasMaxLength(250);
            entity.Property(r => r.DeliveryCompany).HasMaxLength(200);
            entity.Property(r => r.DeliveryAddress).HasMaxLength(250);
            entity.Property(r => r.LoadDescription).HasMaxLength(500);

            entity.Property(r => r.RateValue).HasColumnType("decimal(18,2)");
            entity.Property(r => r.Quantity).HasColumnType("decimal(18,2)");
            entity.Property(r => r.Total).HasColumnType("decimal(18,2)");

            entity.Property(r => r.SignatureJson).IsRequired();
            entity.Property(r => r.ReceiverName).IsRequired().HasMaxLength(200);

            entity.Property(r => r.ClientCompanyName).IsRequired().HasMaxLength(200);
            entity.Property(r => r.ClientContactName).HasMaxLength(200);
            entity.Property(r => r.ClientEmail).HasMaxLength(320);
            entity.Property(r => r.ClientAddressLine1).IsRequired().HasMaxLength(250);
            entity.Property(r => r.ClientCity).IsRequired().HasMaxLength(120);
            entity.Property(r => r.ClientCountry).IsRequired().HasMaxLength(120);

            entity.HasIndex(r => r.DeliveredAtUtc);
        });
    }

    #endregion

    #region Entity Config: WorkSite

    private static void ConfigureWorkSites(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkSite>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.OwnerUserId).IsRequired();

            entity.Property(x => x.Name)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(x => x.CompanyName)
                  .HasMaxLength(200);

            entity.Property(x => x.IsActive)
                  .IsRequired();

            entity.Property(x => x.AddressLine1).HasMaxLength(250);
            entity.Property(x => x.AddressLine2).HasMaxLength(250);
            entity.Property(x => x.Suburb).HasMaxLength(120);
            entity.Property(x => x.City).HasMaxLength(120);
            entity.Property(x => x.Region).HasMaxLength(120);
            entity.Property(x => x.Postcode).HasMaxLength(20);
            entity.Property(x => x.Country).HasMaxLength(120);

            entity.HasIndex(x => x.OwnerUserId);
            entity.HasIndex(x => new { x.OwnerUserId, x.IsActive });
            entity.HasIndex(x => new { x.OwnerUserId, x.Name });
            entity.HasIndex(x => new { x.OwnerUserId, x.CompanyName });
        });
    }

    #endregion

    #region Entity Config: InductionRequirement

    private static void ConfigureInductionRequirements(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InductionRequirement>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.OwnerUserId).IsRequired();
            entity.Property(x => x.WorkSiteId).IsRequired();

            entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
            entity.Property(x => x.ValidForDays);
            entity.Property(x => x.IsActive).IsRequired();

            entity.Property(x => x.PpeRequired).HasMaxLength(200);

            entity.Property(x => x.CompanyName).HasMaxLength(200);
            entity.HasIndex(x => new { x.OwnerUserId, x.CompanyName });
            entity.HasIndex(x => x.OwnerUserId);
            entity.HasIndex(x => x.WorkSiteId);
            entity.HasIndex(x => new { x.OwnerUserId, x.WorkSiteId, x.IsActive });

            entity.HasOne<WorkSite>()
                  .WithMany()
                  .HasForeignKey(x => x.WorkSiteId)
                  .OnDelete(DeleteBehavior.NoAction);
        });
    }

    #endregion

    #region Entity Config: DriverInduction

    private static void ConfigureDriverInductions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DriverInduction>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.OwnerUserId).IsRequired();
            entity.Property(x => x.DriverId).IsRequired();
            entity.Property(x => x.WorkSiteId).IsRequired();
            entity.Property(x => x.RequirementId).IsRequired();

            entity.Property(x => x.IssueDateUtc).IsRequired();

            entity.Property(x => x.EvidenceFileName)
                  .HasMaxLength(260);

            entity.Property(x => x.EvidenceContentType)
                  .HasMaxLength(150);

            entity.Property(x => x.EvidenceFilePath)
                  .HasMaxLength(1000);

            entity.Property(x => x.EvidenceUploadedOnUtc);

            entity.HasIndex(x => new { x.OwnerUserId, x.DriverId, x.WorkSiteId, x.RequirementId })
                  .IsUnique();

            entity.HasIndex(x => new { x.OwnerUserId, x.DriverId });

            entity.HasOne<Driver>()
                  .WithMany()
                  .HasForeignKey(x => x.DriverId)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne<WorkSite>()
                  .WithMany()
                  .HasForeignKey(x => x.WorkSiteId)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne<InductionRequirement>()
                  .WithMany()
                  .HasForeignKey(x => x.RequirementId)
                  .OnDelete(DeleteBehavior.NoAction);
        });
    }

    #endregion

    #region Entity Config: OdometerReading

    private static void ConfigureOdometerReadings(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OdometerReading>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.VehicleAssetId).IsRequired();
            entity.Property(x => x.UnitNumber).IsRequired();
            entity.Property(x => x.RecordedAtUtc).IsRequired();
            entity.Property(x => x.ReadingKm).IsRequired();
            entity.Property(x => x.ReadingType).IsRequired();

            entity.Property(x => x.Notes)
                  .HasMaxLength(1000);

            entity.HasIndex(x => x.VehicleAssetId);
            entity.HasIndex(x => new { x.VehicleAssetId, x.UnitNumber, x.RecordedAtUtc });

            entity.HasOne<VehicleAsset>()
                  .WithMany()
                  .HasForeignKey(x => x.VehicleAssetId)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne<Driver>()
                  .WithMany()
                  .HasForeignKey(x => x.DriverId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne<UserAccount>()
                  .WithMany()
                  .HasForeignKey(x => x.RecordedByUserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }

    #endregion
}