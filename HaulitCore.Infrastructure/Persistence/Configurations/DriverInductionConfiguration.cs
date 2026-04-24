using HaulitCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HaulitCore.Infrastructure.Persistence.Configurations;

public sealed class DriverInductionConfiguration : IEntityTypeConfiguration<DriverInduction>
{
    public void Configure(EntityTypeBuilder<DriverInduction> entity)
    {
        entity.HasKey(x => x.Id);

        entity.Property(x => x.OwnerUserId).IsRequired();
        entity.Property(x => x.DriverId).IsRequired();
        entity.Property(x => x.WorkSiteId).IsRequired();
        entity.Property(x => x.RequirementId).IsRequired();
        entity.Property(x => x.IssueDateUtc).IsRequired();
        entity.Property(x => x.EvidenceFileName).HasMaxLength(260);
        entity.Property(x => x.EvidenceContentType).HasMaxLength(150);
        entity.Property(x => x.EvidenceFilePath).HasMaxLength(1000);
        entity.Property(x => x.EvidenceUploadedOnUtc);

        entity.HasIndex(x => new { x.OwnerUserId, x.DriverId, x.WorkSiteId, x.RequirementId }).IsUnique();
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
    }
}
