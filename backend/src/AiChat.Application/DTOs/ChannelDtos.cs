using AiChat.Domain.Aggregates.ChannelAggregate;

namespace AiChat.Application.DTOs;

// ========== Channel DTOs ==========

public record ChannelDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public string BaseUrl { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
    public DateTime CreatedAt { get; init; }
    public int ModelCount { get; init; }

    // 负载均衡相关
    public int Priority { get; init; }
    public int Weight { get; init; }
    public int MaxRetries { get; init; }
    public bool IsHealthy { get; init; }
    public DateTime? LastFailedAt { get; init; }
}


public record CreateChannelRequest
{
    public string Name { get; init; } = string.Empty;
    public Provider Provider { get; init; }
    public string ApiKey { get; init; } = string.Empty;
    public string BaseUrl { get; init; } = string.Empty;
}

public record UpdateChannelRequest
{
    public string Name { get; init; } = string.Empty;
    public Provider Provider { get; init; }
    public string? ApiKey { get; init; }  // 可选，不更新则为 null
    public string BaseUrl { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }

    // 负载均衡相关（可选）
    public int? Priority { get; init; }
    public int? Weight { get; init; }
    public int? MaxRetries { get; init; }
}


// ========== AiModel DTOs ==========

public record AiModelDto
{
    public Guid Id { get; init; }
    public Guid ChannelId { get; init; }
    public string ChannelName { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string ModelId { get; init; } = string.Empty;
    public string ModelType { get; init; } = string.Empty;
    public int MaxTokens { get; init; }
    public decimal InputPrice { get; init; }
    public decimal OutputPrice { get; init; }
    public bool IsEnabled { get; init; }
    public int SortOrder { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record CreateAiModelRequest
{
    public Guid ChannelId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ModelId { get; init; } = string.Empty;
    public ModelType ModelType { get; init; }
    public int MaxTokens { get; init; } = 4096;
    public decimal InputPrice { get; init; }
    public decimal OutputPrice { get; init; }
    public int SortOrder { get; init; }
}

public record UpdateAiModelRequest
{
    public string Name { get; init; } = string.Empty;
    public string ModelId { get; init; } = string.Empty;
    public ModelType ModelType { get; init; }
    public int MaxTokens { get; init; }
    public decimal InputPrice { get; init; }
    public decimal OutputPrice { get; init; }
    public bool IsEnabled { get; init; }
    public int SortOrder { get; init; }
}
