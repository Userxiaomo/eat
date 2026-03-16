namespace AiChat.Application.DTOs;

/// <summary>
/// MCP 服务器 DTO
/// </summary>
public record McpServerDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ServerType { get; init; } = "Stdio";
    public string? Command { get; init; }
    public string? Args { get; init; }
    public string? SseUrl { get; init; }
    public string? Environment { get; init; }
    public bool IsEnabled { get; init; }
    public string Description { get; init; } = string.Empty;
    public List<McpToolDto> Tools { get; init; } = new();
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

/// <summary>
/// MCP 工具 DTO
/// </summary>
public record McpToolDto
{
    public Guid Id { get; init; }
    public Guid ServerId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? InputSchema { get; init; }
    public bool IsEnabled { get; init; }
}

/// <summary>
/// 创建 MCP 服务器请求
/// </summary>
public record CreateMcpServerRequest
{
    public string Name { get; init; } = string.Empty;
    public int ServerType { get; init; } // 0 = Stdio, 1 = Sse
    public string? Command { get; init; }
    public string? Args { get; init; }
    public string? SseUrl { get; init; }
    public string? Environment { get; init; }
    public string Description { get; init; } = string.Empty;
}

/// <summary>
/// 更新 MCP 服务器请求
/// </summary>
public record UpdateMcpServerRequest
{
    public string? Name { get; init; }
    public string? Command { get; init; }
    public string? Args { get; init; }
    public string? SseUrl { get; init; }
    public string? Environment { get; init; }
    public bool? IsEnabled { get; init; }
    public string? Description { get; init; }
}
