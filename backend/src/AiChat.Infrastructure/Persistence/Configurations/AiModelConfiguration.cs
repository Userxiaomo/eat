using AiChat.Domain.Aggregates.ChannelAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiChat.Infrastructure.Persistence.Configurations;

public class AiModelConfiguration : IEntityTypeConfiguration<AiModel>
{
    public void Configure(EntityTypeBuilder<AiModel> builder)
    {
        builder.ToTable("AiModels");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.ModelId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.ModelType)
            .IsRequired();

        builder.Property(m => m.MaxTokens)
            .IsRequired()
            .HasDefaultValue(4096);

        builder.Property(m => m.InputPrice)
            .HasPrecision(18, 8);

        builder.Property(m => m.OutputPrice)
            .HasPrecision(18, 8);

        builder.Property(m => m.IsEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(m => m.SortOrder)
            .HasDefaultValue(0);

        builder.Property(m => m.CreatedAt)
            .IsRequired();

        // === 模型能力标识 ===
        builder.Property(m => m.SupportVision)
            .HasDefaultValue(false);

        builder.Property(m => m.SupportTool)
            .HasDefaultValue(false);

        builder.Property(m => m.BuiltInImageGen)
            .HasDefaultValue(false);

        builder.Property(m => m.BuiltInWebSearch)
            .HasDefaultValue(false);

        builder.Property(m => m.Selected)
            .HasDefaultValue(true);

        // 索引
        builder.HasIndex(m => m.ChannelId);
        builder.HasIndex(m => m.ModelType);
        builder.HasIndex(m => new { m.IsEnabled, m.SortOrder });

        // 联合唯一约束：同一渠道下模型 ID 唯一
        builder.HasIndex(m => new { m.ChannelId, m.ModelId })
            .IsUnique();
    }
}
