using HaulitCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HaulitCore.Infrastructure.Persistence.Configurations;

public sealed class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> entity)
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

        entity.Property(j => j.CustomerId).IsRequired(false);
        entity.HasIndex(j => j.CustomerId);

        entity.HasOne<Customer>()
              .WithMany()
              .HasForeignKey(j => j.CustomerId)
              .OnDelete(DeleteBehavior.SetNull);

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

        entity.HasMany(j => j.TrailerAssignments)
              .WithOne()
              .HasForeignKey(t => t.JobId)
              .OnDelete(DeleteBehavior.Cascade);

        entity.Navigation(j => j.TrailerAssignments)
              .HasField("_trailerAssignments");

        entity.Navigation(j => j.TrailerAssignments)
              .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}