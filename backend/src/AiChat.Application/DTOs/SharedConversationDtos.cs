namespace AiChat.Application.DTOs;

// ========== 对话分享 DTOs ==========

/// <summary>
/// 分享的对话预览信息
/// </summary>
public record SharedConversationPreviewDto
{
    public string ShareHash { get; init; } = string.Empty;
    public Guid ConversationId { get; init; }
    public string ConversationName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

/// <summary>
/// 分享的对话完整信息
/// </summary>
public record SharedConversationDto
{
    public string ShareHash { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string ConversationName { get; init; } = string.Empty;
    public string ModelId { get; init; } = string.Empty;
    public IEnumerable<MessageDto> Messages { get; init; } = new List<MessageDto>();
    public DateTime SharedAt { get; init; }
}
