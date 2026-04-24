using HaulitCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HaulitCore.Infrastructure.Persistence.Configurations;

public sealed class JobTrailerAssignmentConfiguration : IEntityTypeConfiguration<JobTrailerAssignment>
{
    public void Configure(EntityTypeBuilder<JobTrailerAssignment> entity)
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
    }
}