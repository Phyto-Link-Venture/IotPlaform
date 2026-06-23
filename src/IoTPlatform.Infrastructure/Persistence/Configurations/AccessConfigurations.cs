using IoTPlatform.Models.Entities.Access;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IoTPlatform.Infrastructure.Persistence.Configurations;

public sealed class ModuleConfiguration : IEntityTypeConfiguration<Module>
{
    public void Configure(EntityTypeBuilder<Module> builder)
    {
        builder.Property(m => m.Code).HasMaxLength(64).IsRequired();
        builder.Property(m => m.Name).HasMaxLength(128).IsRequired();
        builder.HasIndex(m => m.Code).IsUnique();
    }
}

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.Property(p => p.Code).HasMaxLength(128).IsRequired();
        builder.Property(p => p.Name).HasMaxLength(128).IsRequired();
        builder.HasOne(p => p.Module)
            .WithMany(m => m.Permissions)
            .HasForeignKey(p => p.ModuleId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(p => p.Code).IsUnique();
    }
}

public sealed class UserGroupPermissionConfiguration : IEntityTypeConfiguration<UserGroupPermission>
{
    public void Configure(EntityTypeBuilder<UserGroupPermission> builder)
    {
        builder.HasOne(x => x.UserGroup)
            .WithMany(g => g.Permissions)
            .HasForeignKey(x => x.UserGroupId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Permission)
            .WithMany(p => p.UserGroupPermissions)
            .HasForeignKey(x => x.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.UserGroupId, x.PermissionId }).IsUnique();
    }
}
