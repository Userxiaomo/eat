using AiChat.Domain.Aggregates.ConversationAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiChat.Infrastructure.Persistence.Configurations;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("Conversations");

        builder.HasKey(c => c.Id);

        // GUID 由客户端生成
        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        builder.Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.UserId)
            .IsRequired();

        builder.Property(c => c.ModelId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .IsRequired();

        // === Token 统计 ===
        builder.Property(c => c.InputTokens)
            .HasDefaultValue(0);

        builder.Property(c => c.OutputTokens)
            .HasDefaultValue(0);

        // TotalTokens 是计算属性，需忽略
        builder.Ignore(c => c.TotalTokens);

        // === 历史消息策略 ===
        builder.Property(c => c.HistoryType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(HistoryType.Count);

        builder.Property(c => c.HistoryCount)
            .HasDefaultValue(5);

        // === 收藏功能 ===
        builder.Property(c => c.IsStar)
            .HasDefaultValue(false);

        builder.Property(c => c.StarAt);

        // === 搜索功能 ===
        builder.Property(c => c.SearchEnabled)
            .HasDefaultValue(false);

        // === 头像 ===
        builder.Property(c => c.Avatar)
            .HasMaxLength(500);

        builder.Property(c => c.AvatarType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(AvatarType.None);

        // === Bot 关联 ===
        builder.Property(c => c.BotId);

        builder.Property(c => c.IsWithBot)
            .HasDefaultValue(false);

        builder.Property(c => c.DefaultProviderId);

        // 配置值对象 ConversationSettings 为 Owned Type
        builder.OwnsOne(c => c.Settings, settings =>
        {
            settings.Property(s => s.Temperature)
                .IsRequired()
                .HasColumnName("Temperature");

            settings.Property(s => s.MaxTokens)
                .IsRequired()
                .HasColumnName("MaxTokens");

            settings.Property(s => s.SystemPrompt)
                .HasMaxLength(1000)
                .HasColumnName("SystemPrompt");
        });

        // 配置与 Message 的一对多关系
        builder.HasMany(c => c.Messages)
            .WithOne()
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        // 忽略领域事件集合
        builder.Ignore(c => c.DomainEvents);

        // 索引
        builder.HasIndex(c => c.UserId);
        builder.HasIndex(c => c.CreatedAt);
        builder.HasIndex(c => c.IsStar);
    }
}
