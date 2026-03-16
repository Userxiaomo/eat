using AiChat.Domain.Aggregates.BotAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiChat.Infrastructure.Persistence.Configurations;

public class BotConfiguration : IEntityTypeConfiguration<Bot>
{
    public void Configure(EntityTypeBuilder<Bot> builder)
    {
        builder.ToTable("Bots");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.Description)
            .HasMaxLength(500);

        builder.Property(b => b.SystemPrompt)
            .HasMaxLength(10000);

        builder.Property(b => b.AvatarUrl)
            .HasMaxLength(500);

        builder.Property(b => b.IsSystem)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(b => b.IsPublic)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(b => b.EnableWebSearch)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(b => b.SortOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(b => b.CreatedAt).IsRequired();
        builder.Property(b => b.UpdatedAt).IsRequired();

        // 索引
        builder.HasIndex(b => b.IsSystem);
        builder.HasIndex(b => b.CreatedByUserId);
    }
}
