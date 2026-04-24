using HaulitCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HaulitCore.Infrastructure.Persistence.Configurations;

public sealed class DocumentSettingsConfiguration : IEntityTypeConfiguration<DocumentSettings>
{
    public void Configure(EntityTypeBuilder<DocumentSettings> entity)
    {
        entity.HasKey(x => x.Id);

        entity.Property(x => x.OwnerUserId).IsRequired();
        entity.HasIndex(x => x.OwnerUserId).IsUnique();

        entity.Property(x => x.GstRatePercent).HasColumnType("decimal(18,2)");
        entity.Property(x => x.FuelSurchargePercent).HasColumnType("decimal(18,2)");

        entity.Property(x => x.InvoicePrefix).HasMaxLength(20).IsRequired();
        entity.Property(x => x.PodPrefix).HasMaxLength(20).IsRequired();

        entity.Property(x => x.WaitTimeCharge).HasColumnType("decimal(18,2)");
        entity.Property(x => x.HandUnloadCharge).HasColumnType("decimal(18,2)");

    }
}