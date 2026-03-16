using AiChat.Domain.Common;

namespace AiChat.Domain.Aggregates.ConversationAggregate;

/// <summary>
/// 搜索状态枚举
/// </summary>
public enum SearchStatus
{
    /// <summary>
    /// 未搜索
    /// </summary>
    None = 0,

    /// <summary>
    /// 搜索中
    /// </summary>
    Searching = 1,

    /// <summary>
    /// 搜索完成
    /// </summary>
    Done = 2,

    /// <summary>
    /// 搜索失败
    /// </summary>
    Error = 3
}

/// <summary>
/// 消息类型枚举
/// </summary>
public enum MessageType
{
    Text = 0,
    Image = 1,
    Error = 2,
    Break = 3
}

/// <summary>
/// 消息实体
/// </summary>
public class Message : Entity<Guid>
{
    public string Content { get; private set; } = string.Empty;
    public MessageRole Role { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid ConversationId { get; private set; }

    /// <summary>
    /// 消息类型
    /// </summary>
    public MessageType MessageType { get; private set; } = MessageType.Text;

    /// <summary>
    /// 思维链内容（DeepSeek reasoning_content）
    /// </summary>
    public string? ReasoningContent { get; private set; }

    /// <summary>
    /// 输入 Token 数量
    /// </summary>
    public int? InputTokens { get; private set; }

    /// <summary>
    /// 输出 Token 数量
    /// </summary>
    public int? OutputTokens { get; private set; }

    /// <summary>
    /// 使用的模型 ID
    /// </summary>
    public string? Model { get; private set; }

    /// <summary>
    /// 使用的 Provider ID
    /// </summary>
    public Guid? ProviderId { get; private set; }

    // === 搜索相关 ===
    /// <summary>
    /// 是否启用了搜索
    /// </summary>
    public bool SearchEnabled { get; private set; }

    /// <summary>
    /// 搜索状态
    /// </summary>
    public SearchStatus SearchStatus { get; private set; } = SearchStatus.None;

    /// <summary>
    /// Web 搜索结果（JSON 格式存储）
    /// </summary>
    public string? WebSearchResultJson { get; private set; }

    // === MCP 工具调用 ===
    /// <summary>
    /// MCP 工具调用结果（JSON 格式存储）
    /// </summary>
    public string? McpToolsJson { get; private set; }

    // === 错误信息 ===
    /// <summary>
    /// 错误类型
    /// </summary>
    public string? ErrorType { get; private set; }

    /// <summary>
    /// 错误消息
    /// </summary>
    public string? ErrorMessage { get; private set; }

    // === 软删除 ===
    /// <summary>
    /// 删除时间（软删除）
    /// </summary>
    public DateTime? DeletedAt { get; private set; }

    /// <summary>
    /// 是否已删除
    /// </summary>
    public bool IsDeleted => DeletedAt.HasValue;

    // EF Core需要的无参构造函数
    private Message()
    {
    }

    public Message(Guid id, string content, MessageRole role, Guid conversationId, string? reasoningContent = null)
        : base(id)
    {
        Content = content ?? throw new ArgumentNullException(nameof(content));
        Role = role;
        ConversationId = conversationId;
        ReasoningContent = reasoningContent;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateContent(string newContent)
    {
        if (string.IsNullOrWhiteSpace(newContent))
            throw new ArgumentException("Message content cannot be empty.", nameof(newContent));

        Content = newContent;
    }

    public void SetReasoningContent(string? reasoning)
    {
        ReasoningContent = reasoning;
    }

    public void SetTokenUsage(int? inputTokens, int? outputTokens)
    {
        InputTokens = inputTokens;
        OutputTokens = outputTokens;
    }

    /// <summary>
    /// 设置模型和 Provider
    /// </summary>
    public void SetModelInfo(string? model, Guid? providerId)
    {
        Model = model;
        ProviderId = providerId;
    }

    /// <summary>
    /// 设置搜索状态
    /// </summary>
    public void SetSearchStatus(SearchStatus status)
    {
        SearchStatus = status;
    }

    /// <summary>
    /// 设置搜索结果
    /// </summary>
    public void SetWebSearchResult(string? resultJson)
    {
        WebSearchResultJson = resultJson;
        if (!string.IsNullOrEmpty(resultJson))
        {
            SearchStatus = SearchStatus.Done;
        }
    }

    /// <summary>
    /// 启用搜索
    /// </summary>
    public void EnableSearch()
    {
        SearchEnabled = true;
    }

    /// <summary>
    /// 设置 MCP 工具调用结果
    /// </summary>
    public void SetMcpToolsResult(string? toolsJson)
    {
        McpToolsJson = toolsJson;
    }

    /// <summary>
    /// 设置错误信息
    /// </summary>
    public void SetError(string? errorType, string? errorMessage)
    {
        MessageType = MessageType.Error;
        ErrorType = errorType;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// 软删除消息
    /// </summary>
    public void SoftDelete()
    {
        DeletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 恢复删除的消息
    /// </summary>
    public void Restore()
    {
        DeletedAt = null;
    }
}
