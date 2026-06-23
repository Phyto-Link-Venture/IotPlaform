using IoTPlatform.Models.Entities.Ai;
using IoTPlatform.Models.Entities.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IoTPlatform.Infrastructure.Persistence.Configurations;

public sealed class AiProviderConfigConfiguration : IEntityTypeConfiguration<AiProviderConfig>
{
    public void Configure(EntityTypeBuilder<AiProviderConfig> builder)
    {
        builder.Property(c => c.Provider).HasMaxLength(64).IsRequired();
        builder.Property(c => c.Model).HasMaxLength(128).IsRequired();
        builder.Property(c => c.ApiKeyEncrypted).IsRequired();
        builder.HasIndex(c => new { c.CompanyId, c.Provider, c.IsActive });
    }
}

public sealed class AiConversationConfiguration : IEntityTypeConfiguration<AiConversation>
{
    public void Configure(EntityTypeBuilder<AiConversation> builder)
    {
        builder.Property(c => c.Title).HasMaxLength(256);
        builder.HasIndex(c => new { c.CompanyId, c.UserId });
    }
}

public sealed class AiMessageConfiguration : IEntityTypeConfiguration<AiMessage>
{
    public void Configure(EntityTypeBuilder<AiMessage> builder)
    {
        builder.Property(m => m.ToolCalls).HasColumnType("jsonb");
        builder.Property(m => m.Model).HasMaxLength(128);
        builder.HasOne(m => m.Conversation)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(m => m.ConversationId);
    }
}

public sealed class McpClientConfiguration : IEntityTypeConfiguration<McpClient>
{
    public void Configure(EntityTypeBuilder<McpClient> builder)
    {
        builder.Property(c => c.Name).HasMaxLength(256).IsRequired();
        builder.Property(c => c.ApiKeyHash).IsRequired();
        builder.HasIndex(c => c.ApiKeyHash);
    }
}

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.EntityType).HasMaxLength(128).IsRequired();
        builder.Property(a => a.Action).HasMaxLength(64).IsRequired();
        builder.Property(a => a.OldValue).HasColumnType("jsonb");
        builder.Property(a => a.NewValue).HasColumnType("jsonb");
        builder.HasIndex(a => new { a.CompanyId, a.Timestamp });
        builder.HasIndex(a => new { a.EntityType, a.EntityId });
    }
}

public sealed class SystemLogConfiguration : IEntityTypeConfiguration<SystemLog>
{
    public void Configure(EntityTypeBuilder<SystemLog> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Level).HasMaxLength(32).IsRequired();
        builder.Property(s => s.Properties).HasColumnType("jsonb");
        builder.HasIndex(s => new { s.Level, s.Timestamp });
    }
}
