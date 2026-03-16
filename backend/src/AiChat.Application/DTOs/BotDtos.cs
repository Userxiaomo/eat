namespace AiChat.Application.DTOs;

/// <summary>
/// Bot DTO
/// </summary>
public record BotDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string SystemPrompt { get; init; } = string.Empty;
    public string? AvatarUrl { get; init; }
    public Guid? ModelId { get; init; }
    public bool IsSystem { get; init; }
    public Guid? CreatedByUserId { get; init; }
    public bool IsPublic { get; init; }
    public bool EnableWebSearch { get; init; }
    public int SortOrder { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

/// <summary>
/// 创建 Bot 请求
/// </summary>
public record CreateBotRequest
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string SystemPrompt { get; init; } = string.Empty;
    public string? AvatarUrl { get; init; }
    public Guid? ModelId { get; init; }
    public bool IsPublic { get; init; }
    public bool EnableWebSearch { get; init; }
}

/// <summary>
/// 更新 Bot 请求
/// </summary>
public record UpdateBotRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? SystemPrompt { get; init; }
    public string? AvatarUrl { get; init; }
    public Guid? ModelId { get; init; }
    public bool? IsPublic { get; init; }
    public bool? EnableWebSearch { get; init; }
    public int? SortOrder { get; init; }
}

/// <summary>
/// 搜索引擎配置 DTO
/// </summary>
public record SearchEngineConfigDto
{
    public Guid Id { get; init; }
    public string EngineType { get; init; } = "Tavily";
    public string Name { get; init; } = string.Empty;
    public string? ApiUrl { get; init; }
    public bool IsEnabled { get; init; }
    public bool IsDefault { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// 创建/更新搜索引擎配置请求
/// </summary>
public record SaveSearchEngineConfigRequest
{
    public int EngineType { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ApiKey { get; init; } = string.Empty;
    public string? ApiUrl { get; init; }
    public bool IsEnabled { get; init; } = true;
    public bool IsDefault { get; init; }
}

/// <summary>
/// 网络搜索结果
/// </summary>
public record WebSearchResult
{
    public string Title { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string Snippet { get; init; } = string.Empty;
}
