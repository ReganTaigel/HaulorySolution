using Haulory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Haulory.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> entity)
    {
        entity.HasKey(x => x.Id);

        entity.Property(x => x.OwnerUserId).IsRequired();
        entity.HasIndex(x => x.OwnerUserId);

        entity.Property(x => x.CompanyName).IsRequired().HasMaxLength(200);
        entity.Property(x => x.ContactName).HasMaxLength(200);
        entity.Property(x => x.Email).HasMaxLength(320);
        entity.Property(x => x.AddressLine1).IsRequired().HasMaxLength(250);
        entity.Property(x => x.City).IsRequired().HasMaxLength(120);
        entity.Property(x => x.Country).IsRequired().HasMaxLength(120);

        entity.Property(x => x.CreatedAtUtc).IsRequired();
        entity.Property(x => x.UpdatedAtUtc).IsRequired(false);

        entity.HasIndex(x => new { x.OwnerUserId, x.CompanyName });

        entity.HasOne<UserAccount>()
            .WithMany()
            .HasForeignKey(x => x.OwnerUserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}