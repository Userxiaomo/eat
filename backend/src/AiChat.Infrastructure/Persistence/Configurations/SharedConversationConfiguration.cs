using AiChat.Domain.Aggregates.ConversationAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiChat.Infrastructure.Persistence.Configurations;

public class SharedConversationConfiguration : IEntityTypeConfiguration<SharedConversation>
{
    public void Configure(EntityTypeBuilder<SharedConversation> builder)
    {
        builder.ToTable("SharedConversations");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.ShareHash)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(s => s.UserId)
            .IsRequired();

        builder.Property(s => s.ConversationId)
            .IsRequired();

        builder.Property(s => s.MessageIds)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .IsRequired();

        // 索引：ShareHash 唯一
        builder.HasIndex(s => s.ShareHash)
            .IsUnique();

        // 索引：UserId（用于查询用户的分享列表）
        builder.HasIndex(s => s.UserId);
    }
}
