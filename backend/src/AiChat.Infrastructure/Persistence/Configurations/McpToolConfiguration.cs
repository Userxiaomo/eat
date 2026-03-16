using AiChat.Domain.Aggregates.McpAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiChat.Infrastructure.Persistence.Configurations;

public class McpToolConfiguration : IEntityTypeConfiguration<McpTool>
{
    public void Configure(EntityTypeBuilder<McpTool> builder)
    {
        builder.ToTable("McpTools");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.ServerId)
            .IsRequired();

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Description)
            .HasMaxLength(500);

        builder.Property(t => t.InputSchema)
            .HasMaxLength(10000);

        builder.Property(t => t.IsEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.UpdatedAt).IsRequired();

        builder.HasIndex(t => t.ServerId);
    }
}
