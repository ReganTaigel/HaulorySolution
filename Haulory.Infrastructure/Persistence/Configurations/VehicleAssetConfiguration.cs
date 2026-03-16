using Haulory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Haulory.Infrastructure.Persistence.Configurations;

public sealed class VehicleAssetConfiguration : IEntityTypeConfiguration<VehicleAsset>
{
    public void Configure(EntityTypeBuilder<VehicleAsset> entity)
    {
        entity.HasKey(v => v.Id);

        entity.Property(v => v.OwnerUserId).IsRequired();

        entity.HasOne<UserAccount>()
              .WithMany()
              .HasForeignKey(v => v.OwnerUserId)
              .OnDelete(DeleteBehavior.NoAction);

        entity.HasIndex(v => v.OwnerUserId);
        entity.HasIndex(v => v.VehicleSetId);

        entity.Property(v => v.Rego).IsRequired().HasMaxLength(32);
        entity.HasIndex(v => new { v.OwnerUserId, v.Rego }).IsUnique();
        entity.Property(v => v.Make).IsRequired().HasMaxLength(120);
        entity.Property(v => v.Model).IsRequired().HasMaxLength(120);
        entity.Property(v => v.Year).IsRequired();
        entity.Property(v => v.CreatedUtc).IsRequired();
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

        entity.HasIndex(v => new { v.VehicleSetId, v.UnitNumber }).IsUnique();
    }
}
