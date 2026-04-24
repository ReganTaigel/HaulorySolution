using HaulitCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HaulitCore.Infrastructure.Persistence.Configurations;

public sealed class DeliveryReceiptConfiguration : IEntityTypeConfiguration<DeliveryReceipt>
{
    public void Configure(EntityTypeBuilder<DeliveryReceipt> entity)
    {
        entity.HasKey(r => r.Id);

        entity.Property(r => r.OwnerUserId).IsRequired();
        entity.HasIndex(r => r.OwnerUserId);
        entity.HasIndex(r => new { r.OwnerUserId, r.JobId }).IsUnique();

        entity.Property(r => r.ReferenceNumber).HasMaxLength(64);
        entity.Property(r => r.InvoiceNumber).HasMaxLength(64);
        entity.Property(r => r.PickupCompany).HasMaxLength(200);
        entity.Property(r => r.PickupAddress).HasMaxLength(250);
        entity.Property(r => r.DeliveryCompany).HasMaxLength(200);
        entity.Property(r => r.DeliveryAddress).HasMaxLength(250);
        entity.Property(r => r.LoadDescription).HasMaxLength(500);

        entity.Property(r => r.RateValue).HasColumnType("decimal(18,2)");
        entity.Property(r => r.Quantity).HasColumnType("decimal(18,2)");

        entity.Property(r => r.Subtotal).HasColumnType("decimal(18,2)");
        entity.Property(r => r.GstRatePercent).HasColumnType("decimal(18,2)");
        entity.Property(r => r.GstAmount).HasColumnType("decimal(18,2)");
        entity.Property(r => r.FuelSurchargePercent).HasColumnType("decimal(18,2)");
        entity.Property(r => r.FuelSurchargeAmount).HasColumnType("decimal(18,2)");
        entity.Property(r => r.WaitTimeChargeAmount).HasColumnType("decimal(18,2)");
        entity.Property(r => r.HandUnloadChargeAmount).HasColumnType("decimal(18,2)");
        entity.Property(r => r.Total).HasColumnType("decimal(18,2)");

        entity.Property(r => r.SignatureJson).IsRequired();
        entity.Property(r => r.ReceiverName).IsRequired().HasMaxLength(200);

        entity.Property(r => r.ClientCompanyName).IsRequired().HasMaxLength(200);
        entity.Property(r => r.ClientContactName).HasMaxLength(200);
        entity.Property(r => r.ClientEmail).HasMaxLength(320);
        entity.Property(r => r.ClientAddressLine1).IsRequired().HasMaxLength(250);
        entity.Property(r => r.ClientCity).IsRequired().HasMaxLength(120);
        entity.Property(r => r.ClientCountry).IsRequired().HasMaxLength(120);

        entity.Property(r => r.DamageNotes).HasMaxLength(1000);

        entity.HasIndex(r => r.DeliveredAtUtc);
    }
}