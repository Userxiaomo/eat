using AiChat.Domain.Common;

namespace AiChat.Domain.Aggregates.McpAggregate;

/// <summary>
/// MCP 工具实体
/// </summary>
public class McpTool : Entity<Guid>
{
    /// <summary>
    /// 所属服务器 ID
    /// </summary>
    public Guid ServerId { get; private set; }

    /// <summary>
    /// 工具名称
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 工具描述
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// 输入参数 Schema（JSON Schema 格式）
    /// </summary>
    public string? InputSchema { get; private set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; private set; } = true;

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private McpTool() { }

    public McpTool(Guid id, Guid serverId, string name, string description) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tool name cannot be empty.", nameof(name));

        ServerId = serverId;
        Name = name;
        Description = description ?? string.Empty;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateInfo(string name, string description, string? inputSchema)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tool name cannot be empty.", nameof(name));

        Name = name;
        Description = description ?? string.Empty;
        InputSchema = inputSchema;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetEnabled(bool enabled)
    {
        IsEnabled = enabled;
        UpdatedAt = DateTime.UtcNow;
    }
}
