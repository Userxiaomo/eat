using AiChat.Domain.Aggregates.ConversationAggregate;

namespace AiChat.Application.DTOs;

public record ConversationDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string ModelId { get; init; } = string.Empty;
    public double Temperature { get; init; }
    public int MaxTokens { get; init; }
    public string? SystemPrompt { get; init; }

    public bool IsStar { get; init; }
    public DateTime? StarAt { get; init; }
    public bool SearchEnabled { get; init; }
    public string? Avatar { get; init; }
    public AvatarType AvatarType { get; init; }
    public HistoryType HistoryType { get; init; }
    public int HistoryCount { get; init; }
    public int TotalTokens { get; init; }

    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public record MessageDto
{
    public Guid Id { get; init; }
    public Guid ConversationId { get; init; }
    public string Content { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    /// <summary>
    /// 思维链内容（DeepSeek reasoning_content）
    /// </summary>
    public string? ReasoningContent { get; init; }
    /// <summary>
    /// 输入 Token 数量
    /// </summary>
    public int? InputTokens { get; init; }
    /// <summary>
    /// 输出 Token 数量
    /// </summary>
    public int? OutputTokens { get; init; }

    public string? WebSearchResultJson { get; init; }
    public SearchStatus SearchStatus { get; init; }
    public string? McpToolsJson { get; init; }

    public DateTime CreatedAt { get; init; }
}

public record CreateConversationRequest
{
    public string Title { get; init; } = string.Empty;
    public string ModelId { get; init; } = string.Empty;
    public double? Temperature { get; init; }
    public int? MaxTokens { get; init; }
    public string? SystemPrompt { get; init; }
}

public record UpdateConversationRequest
{
    public string? Title { get; init; }
    public string? ModelId { get; init; }
    public double? Temperature { get; init; }
    public int? MaxTokens { get; init; }
    public string? SystemPrompt { get; init; }
}

public record SendMessageRequest
{
    public string Content { get; init; } = string.Empty;
}

public record SendMessageResponse
{
    public MessageDto UserMessage { get; init; } = null!;
    public MessageDto AssistantMessage { get; init; } = null!;
}

public record SetHistoryTypeRequest
{
    public HistoryType Type { get; init; }
    public int Count { get; init; }
}

public record SetAvatarRequest
{
    public string? Avatar { get; init; }
    public AvatarType Type { get; init; }
}

/// <summary>
/// 分页结果
/// </summary>
public record PagedResult<T>
{
    public IEnumerable<T> Items { get; init; } = Enumerable.Empty<T>();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}

