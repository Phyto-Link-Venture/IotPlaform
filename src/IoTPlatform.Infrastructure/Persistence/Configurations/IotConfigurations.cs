using IoTPlatform.Common.Constants;
using IoTPlatform.Models.Entities.Iot;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IoTPlatform.Infrastructure.Persistence.Configurations;

public sealed class DeviceTypeConfiguration : IEntityTypeConfiguration<DeviceType>
{
    public void Configure(EntityTypeBuilder<DeviceType> builder)
    {
        builder.Property(t => t.Code).HasMaxLength(64).IsRequired();
        builder.Property(t => t.Name).HasMaxLength(128).IsRequired();
        builder.HasIndex(t => t.Code).IsUnique();
    }
}

public sealed class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.Property(d => d.DeviceKey).HasMaxLength(128).IsRequired();
        builder.Property(d => d.Name).HasMaxLength(256).IsRequired();
        builder.HasOne(d => d.DeviceType)
            .WithMany(t => t.Devices)
            .HasForeignKey(d => d.DeviceTypeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(d => new { d.CompanyId, d.DeviceKey }).IsUnique();
        builder.HasIndex(d => d.DepartmentId);
    }
}

public sealed class TelemetryDataConfiguration : IEntityTypeConfiguration<TelemetryData>
{
    public void Configure(EntityTypeBuilder<TelemetryData> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.DataNameSymbol)
            .HasMaxLength(TelemetryConventions.DataNameSymbolMaxLength)
            .IsRequired();
        builder.Property(t => t.Metadata).HasColumnType("jsonb");
        builder.HasOne(t => t.Device)
            .WithMany()
            .HasForeignKey(t => t.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);
        // Primary time-series access path: telemetry posts directly at the device level.
        builder.HasIndex(t => new { t.DeviceId, t.DataNameSymbol, t.Timestamp });
    }
}

public sealed class DeviceCommandConfiguration : IEntityTypeConfiguration<DeviceCommand>
{
    public void Configure(EntityTypeBuilder<DeviceCommand> builder)
    {
        builder.Property(c => c.Command).HasMaxLength(128).IsRequired();
        builder.Property(c => c.Payload).HasColumnType("jsonb");
        builder.HasOne(c => c.Device)
            .WithMany(d => d.Commands)
            .HasForeignKey(c => c.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(c => new { c.DeviceId, c.Status });
    }
}
