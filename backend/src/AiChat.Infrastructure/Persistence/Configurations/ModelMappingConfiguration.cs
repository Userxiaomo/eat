using AiChat.Domain.Aggregates.ChannelAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiChat.Infrastructure.Persistence.Configurations;

public class ModelMappingConfiguration : IEntityTypeConfiguration<ModelMapping>
{
    public void Configure(EntityTypeBuilder<ModelMapping> builder)
    {
        builder.ToTable("ModelMappings");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.FromModel)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.ToModel)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.HideUpstream)
            .IsRequired();

        builder.Property(m => m.CreatedAt)
            .IsRequired();

        builder.Property(m => m.UpdatedAt);

        // 索引：渠道 + FromModel 唯一
        builder.HasIndex(m => new { m.ChannelId, m.FromModel })
            .IsUnique();
    }
}
