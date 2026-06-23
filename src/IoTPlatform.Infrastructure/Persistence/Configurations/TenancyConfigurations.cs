using IoTPlatform.Models.Entities.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IoTPlatform.Infrastructure.Persistence.Configurations;

public sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.Property(c => c.Name).HasMaxLength(256).IsRequired();
        builder.Property(c => c.Code).HasMaxLength(64).IsRequired();
        builder.HasIndex(c => c.Code).IsUnique();
    }
}

public sealed class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.Property(d => d.Name).HasMaxLength(256).IsRequired();
        builder.HasOne(d => d.Company)
            .WithMany(c => c.Departments)
            .HasForeignKey(d => d.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(d => new { d.CompanyId, d.Name });
    }
}

public sealed class UserGroupConfiguration : IEntityTypeConfiguration<UserGroup>
{
    public void Configure(EntityTypeBuilder<UserGroup> builder)
    {
        builder.Property(g => g.Name).HasMaxLength(256).IsRequired();
        builder.HasOne(g => g.Department)
            .WithMany(d => d.UserGroups)
            .HasForeignKey(g => g.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(g => new { g.CompanyId, g.DepartmentId, g.Name });
    }
}

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.Email).HasMaxLength(320).IsRequired();
        builder.Property(u => u.FirstName).HasMaxLength(128).IsRequired();
        builder.Property(u => u.LastName).HasMaxLength(128).IsRequired();
        builder.Property(u => u.PasswordHash).IsRequired();
        builder.HasOne(u => u.Company)
            .WithMany(c => c.Users)
            .HasForeignKey(u => u.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(u => new { u.CompanyId, u.Email }).IsUnique();
    }
}

public sealed class UserGroupMemberConfiguration : IEntityTypeConfiguration<UserGroupMember>
{
    public void Configure(EntityTypeBuilder<UserGroupMember> builder)
    {
        builder.HasOne(m => m.User)
            .WithMany(u => u.GroupMemberships)
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.UserGroup)
            .WithMany(g => g.Members)
            .HasForeignKey(m => m.UserGroupId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(m => new { m.UserId, m.UserGroupId }).IsUnique();
    }
}
