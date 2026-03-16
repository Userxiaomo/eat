using AiChat.Domain.Aggregates.ChannelAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiChat.Infrastructure.Persistence.Configurations;

public class ChannelConfiguration : IEntityTypeConfiguration<Channel>
{
    public void Configure(EntityTypeBuilder<Channel> builder)
    {
        builder.ToTable("Channels");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Provider)
            .IsRequired();

        builder.Property(c => c.ApiKey)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(c => c.BaseUrl)
            .HasMaxLength(500);

        builder.Property(c => c.IsEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        // === 新增字段 ===
        builder.Property(c => c.ApiStyle)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(ApiStyle.OpenAI);

        builder.Property(c => c.ChannelType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(ChannelType.Default);

        builder.Property(c => c.Logo)
            .HasMaxLength(2048);

        builder.Property(c => c.SortOrder)
            .HasDefaultValue(0);

        // 一对多关系：Channel -> Models
        builder.HasMany(c => c.Models)
            .WithOne()
            .HasForeignKey(m => m.ChannelId)
            .OnDelete(DeleteBehavior.Cascade);

        // 索引
        builder.HasIndex(c => c.Provider);
        builder.HasIndex(c => new { c.IsEnabled, c.SortOrder });
    }
}
