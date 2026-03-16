using AiChat.Domain.Aggregates.McpAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiChat.Infrastructure.Persistence.Configurations;

public class McpServerConfiguration : IEntityTypeConfiguration<McpServer>
{
    public void Configure(EntityTypeBuilder<McpServer> builder)
    {
        builder.ToTable("McpServers");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.ServerType)
            .IsRequired();

        builder.Property(s => s.Command)
            .HasMaxLength(500);

        builder.Property(s => s.Args)
            .HasMaxLength(2000);

        builder.Property(s => s.SseUrl)
            .HasMaxLength(500);

        builder.Property(s => s.Environment)
            .HasMaxLength(5000);

        builder.Property(s => s.Description)
            .HasMaxLength(500);

        builder.Property(s => s.IsEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.UpdatedAt).IsRequired();

        // 配置与 McpTool 的关系
        builder.HasMany(s => s.Tools)
            .WithOne()
            .HasForeignKey(t => t.ServerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
