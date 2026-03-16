using AiChat.Domain.Common;

namespace AiChat.Domain.Aggregates.ConversationAggregate;

/// <summary>
/// 分享的对话实体
/// </summary>
public class SharedConversation : Entity<Guid>
{
    /// <summary>
    /// 分享的唯一标识（用于生成分享链接）
    /// </summary>
    public string ShareHash { get; private set; } = string.Empty;

    /// <summary>
    /// 分享者用户ID
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// 被分享的对话ID
    /// </summary>
    public Guid ConversationId { get; private set; }

    /// <summary>
    /// 分享的消息ID列表（JSON格式，-1表示全部消息）
    /// </summary>
    public string MessageIds { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // EF Core 需要
    private SharedConversation() { }

    public SharedConversation(
        Guid id,
        string shareHash,
        Guid userId,
        Guid conversationId,
        List<int> messageIds)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(shareHash))
            throw new ArgumentException("ShareHash cannot be empty.", nameof(shareHash));

        ShareHash = shareHash;
        UserId = userId;
        ConversationId = conversationId;
        MessageIds = System.Text.Json.JsonSerializer.Serialize(messageIds);
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateMessageIds(List<int> messageIds)
    {
        MessageIds = System.Text.Json.JsonSerializer.Serialize(messageIds);
        UpdatedAt = DateTime.UtcNow;
    }

    public List<int> GetMessageIds()
    {
        if (string.IsNullOrWhiteSpace(MessageIds))
            return new List<int>();

        return System.Text.Json.JsonSerializer.Deserialize<List<int>>(MessageIds) ?? new List<int>();
    }
}
