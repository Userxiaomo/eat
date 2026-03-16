namespace AiChat.Application.DTOs;

// ========== ModelMapping DTOs ==========

public record ModelMappingDto
{
    public Guid Id { get; init; }
    public Guid ChannelId { get; init; }
    public string ChannelName { get; init; } = string.Empty;
    public string FromModel { get; init; } = string.Empty;
    public string ToModel { get; init; } = string.Empty;
    public bool HideUpstream { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record CreateModelMappingRequest
{
    public Guid ChannelId { get; init; }
    public string FromModel { get; init; } = string.Empty;
    public string ToModel { get; init; } = string.Empty;
    public bool HideUpstream { get; init; }
}

public record UpdateModelMappingRequest
{
    public string FromModel { get; init; } = string.Empty;
    public string ToModel { get; init; } = string.Empty;
    public bool HideUpstream { get; init; }
}
