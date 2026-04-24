using HaulitCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HaulitCore.Infrastructure.Persistence.Configurations;

public sealed class HubodometerReadingConfiguration : IEntityTypeConfiguration<HubodometerReading>
{
    public void Configure(EntityTypeBuilder<HubodometerReading> entity)
    {
        entity.HasKey(x => x.Id);

        entity.Property(x => x.VehicleAssetId).IsRequired();
        entity.Property(x => x.UnitNumber).IsRequired();
        entity.Property(x => x.RecordedAtUtc).IsRequired();
        entity.Property(x => x.ReadingKm).IsRequired();
        entity.Property(x => x.ReadingType).IsRequired();
        entity.Property(x => x.Notes).HasMaxLength(1000);

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
    }
}
