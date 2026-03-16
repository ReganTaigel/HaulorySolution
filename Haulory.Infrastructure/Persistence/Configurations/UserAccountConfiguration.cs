using Haulory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Haulory.Infrastructure.Persistence.Configurations;

public sealed class UserAccountConfiguration : IEntityTypeConfiguration<UserAccount>
{
    public void Configure(EntityTypeBuilder<UserAccount> entity)
    {
        entity.HasKey(x => x.Id);

        entity.HasIndex(x => x.Email).IsUnique();
        entity.HasIndex(x => x.ParentMainUserId);

        entity.Property(x => x.FirstName).IsRequired().HasMaxLength(120);
        entity.Property(x => x.LastName).IsRequired().HasMaxLength(120);
        entity.Property(x => x.Email).IsRequired().HasMaxLength(320);
        entity.Property(x => x.PasswordHash).IsRequired().HasMaxLength(512);
        entity.Property(x => x.Role).IsRequired();

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
    }
}