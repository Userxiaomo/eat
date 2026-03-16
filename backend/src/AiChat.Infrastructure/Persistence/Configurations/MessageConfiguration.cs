using AiChat.Domain.Aggregates.ConversationAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiChat.Infrastructure.Persistence.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");

        builder.HasKey(m => m.Id);

        // GUID 由客户端生成，不是数据库
        builder.Property(m => m.Id)
            .ValueGeneratedNever();

        builder.Property(m => m.Content)
            .IsRequired();

        builder.Property(m => m.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(m => m.CreatedAt)
            .IsRequired();

        builder.Property(m => m.ConversationId)
            .IsRequired();

        // === 消息类型 ===
        builder.Property(m => m.MessageType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(MessageType.Text);

        // === 思维链 ===
        builder.Property(m => m.ReasoningContent);

        // === Token 统计 ===
        builder.Property(m => m.InputTokens);
        builder.Property(m => m.OutputTokens);

        // === 模型信息 ===
        builder.Property(m => m.Model)
            .HasMaxLength(100);

        builder.Property(m => m.ProviderId);

        // === 搜索相关 ===
        builder.Property(m => m.SearchEnabled)
            .HasDefaultValue(false);

        builder.Property(m => m.SearchStatus)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(SearchStatus.None);

        builder.Property(m => m.WebSearchResultJson)
            .HasColumnType("text");

        // === MCP 工具调用 ===
        builder.Property(m => m.McpToolsJson)
            .HasColumnType("text");

        // === 错误信息 ===
        builder.Property(m => m.ErrorType)
            .HasMaxLength(100);

        builder.Property(m => m.ErrorMessage)
            .HasMaxLength(1000);

        // === 软删除 ===
        builder.Property(m => m.DeletedAt);

        // IsDeleted 是计算属性，需忽略
        builder.Ignore(m => m.IsDeleted);

        // 索引
        builder.HasIndex(m => m.ConversationId);
        builder.HasIndex(m => m.CreatedAt);
        builder.HasIndex(m => m.DeletedAt);

        // 查询过滤器：默认排除已删除的消息
        builder.HasQueryFilter(m => m.DeletedAt == null);
    }
}
