using HaulitCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HaulitCore.Infrastructure.Persistence.Configurations;

public sealed class DriverConfiguration : IEntityTypeConfiguration<Driver>
{
    public void Configure(EntityTypeBuilder<Driver> entity)
    {
        entity.HasKey(x => x.Id);

        entity.HasOne<UserAccount>()
              .WithMany()
              .HasForeignKey(x => x.OwnerUserId)
              .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne<UserAccount>()
              .WithMany()
              .HasForeignKey(x => x.UserId)
              .OnDelete(DeleteBehavior.Restrict);

        entity.Property(d => d.FirstName).HasMaxLength(120);
        entity.Property(d => d.LastName).HasMaxLength(120);
        entity.Property(d => d.Email).HasMaxLength(320);
        entity.Property(d => d.PhoneNumber).HasMaxLength(50);
        entity.Property(d => d.DateOfBirthUtc);
        entity.Property(d => d.LicenceNumber).HasMaxLength(64);
        entity.Property(d => d.LicenceVersion).HasMaxLength(64);
        entity.Property(d => d.LicenceClassOrEndorsements).HasMaxLength(200);
        entity.Property(d => d.LicenceIssuedOnUtc);
        entity.Property(d => d.LicenceExpiresOnUtc);
        entity.Property(d => d.LicenceConditionsNotes).HasMaxLength(500);
        entity.Property(d => d.Line1).HasMaxLength(250);
        entity.Property(d => d.Suburb).HasMaxLength(120);
        entity.Property(d => d.City).HasMaxLength(120);
        entity.Property(d => d.Region).HasMaxLength(120);
        entity.Property(d => d.Postcode).HasMaxLength(20);
        entity.Property(d => d.Country).HasMaxLength(120);
        entity.Property(d => d.Status).IsRequired();

        entity.OwnsOne(d => d.EmergencyContact, owned =>
        {
            owned.Property(x => x.FirstName).HasMaxLength(120);
            owned.Property(x => x.LastName).HasMaxLength(120);
            owned.Property(x => x.Relationship).HasMaxLength(80);
            owned.Property(x => x.Email).HasMaxLength(320);
            owned.Property(x => x.PhoneNumber).HasMaxLength(50);
            owned.Property(x => x.SecondaryPhoneNumber).HasMaxLength(50);
        });
    }
}
