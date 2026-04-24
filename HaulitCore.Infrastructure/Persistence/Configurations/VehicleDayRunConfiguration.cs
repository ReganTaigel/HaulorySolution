using HaulitCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HaulitCore.Infrastructure.Persistence.Configurations;

public sealed class VehicleDayRunConfiguration : IEntityTypeConfiguration<VehicleDayRun>
{
    public void Configure(EntityTypeBuilder<VehicleDayRun> entity)
    {
        entity.HasKey(x => x.Id);

        entity.Property(x => x.OwnerUserId).IsRequired();
        entity.Property(x => x.UserId).IsRequired();
        entity.Property(x => x.VehicleAssetId).IsRequired();
        entity.Property(x => x.StartHubodometerKm).IsRequired();
        entity.Property(x => x.EndHubodometerKm);
        entity.Property(x => x.StartedAtUtc).IsRequired();
        entity.Property(x => x.FinishedAtUtc);
        entity.Property(x => x.Notes).HasMaxLength(500);

        entity.HasIndex(x => x.OwnerUserId);
        entity.HasIndex(x => x.UserId);
        entity.HasIndex(x => x.VehicleAssetId);
        entity.HasIndex(x => new { x.UserId, x.VehicleAssetId, x.StartedAtUtc });

        entity.HasOne(x => x.OwnerUser)
              .WithMany()
              .HasForeignKey(x => x.OwnerUserId)
              .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne(x => x.User)
              .WithMany()
              .HasForeignKey(x => x.UserId)
              .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne(x => x.VehicleAsset)
              .WithMany()
              .HasForeignKey(x => x.VehicleAssetId)
              .OnDelete(DeleteBehavior.NoAction);
    }
}
