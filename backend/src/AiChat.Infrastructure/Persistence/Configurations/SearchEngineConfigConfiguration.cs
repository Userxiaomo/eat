using AiChat.Domain.Aggregates.SearchAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiChat.Infrastructure.Persistence.Configurations;

public class SearchEngineConfigConfiguration : IEntityTypeConfiguration<SearchEngineConfig>
{
    public void Configure(EntityTypeBuilder<SearchEngineConfig> builder)
    {
        builder.ToTable("SearchEngineConfigs");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.EngineType)
            .IsRequired();

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.ApiKey)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(s => s.ApiUrl)
            .HasMaxLength(500);

        builder.Property(s => s.IsEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(s => s.IsDefault)
            .IsRequired()
            .HasDefaultValue(false);

        // === 新增字段 ===
        builder.Property(s => s.MaxResults)
            .HasDefaultValue(5);

        builder.Property(s => s.ExtractKeywords)
            .HasDefaultValue(false);

        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.UpdatedAt).IsRequired();

        // 索引
        builder.HasIndex(s => s.EngineType);
        builder.HasIndex(s => s.IsEnabled);
    }
}
