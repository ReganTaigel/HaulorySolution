using Haulory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Haulory.Infrastructure.Persistence.Configurations;

public sealed class InductionRequirementConfiguration : IEntityTypeConfiguration<InductionRequirement>
{
    public void Configure(EntityTypeBuilder<InductionRequirement> entity)
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
    }
}
