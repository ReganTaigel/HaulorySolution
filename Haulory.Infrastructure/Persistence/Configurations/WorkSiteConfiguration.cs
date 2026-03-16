using Haulory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Haulory.Infrastructure.Persistence.Configurations;

public sealed class WorkSiteConfiguration : IEntityTypeConfiguration<WorkSite>
{
    public void Configure(EntityTypeBuilder<WorkSite> entity)
    {
        entity.HasKey(x => x.Id);

        entity.Property(x => x.OwnerUserId).IsRequired();
        entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
        entity.Property(x => x.CompanyName).HasMaxLength(200);
        entity.Property(x => x.IsActive).IsRequired();
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
    }
}
