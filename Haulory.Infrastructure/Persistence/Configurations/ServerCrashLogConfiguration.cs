using Haulory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Haulory.Infrastructure.Persistence.Configurations;

public class ServerCrashLogConfiguration : IEntityTypeConfiguration<ServerCrashLog>
{
    public void Configure(EntityTypeBuilder<ServerCrashLog> builder)
    {
        builder.ToTable("ServerCrashLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Source)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Severity)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Message)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(x => x.ExceptionType)
            .HasMaxLength(500);

        builder.Property(x => x.AccountId)
            .HasMaxLength(100);

        builder.Property(x => x.OwnerId)
            .HasMaxLength(100);

        builder.Property(x => x.PageName)
            .HasMaxLength(100);

        builder.Property(x => x.Platform)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.AppVersion)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.AppBuild)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.MobileCrashId)
            .IsUnique();

        builder.HasIndex(x => x.CreatedUtc);
        builder.HasIndex(x => x.ReceivedUtc);
        builder.HasIndex(x => x.Source);
        builder.HasIndex(x => x.Severity);
    }
}