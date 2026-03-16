using AiChat.Domain.Common;

namespace AiChat.Domain.Aggregates.McpAggregate;

/// <summary>
/// MCP 服务器类型
/// </summary>
public enum McpServerType
{
    /// <summary>
    /// 标准输入输出通信
    /// </summary>
    Stdio = 0,

    /// <summary>
    /// SSE（Server-Sent Events）通信
    /// </summary>
    Sse = 1
}

/// <summary>
/// MCP 服务器配置聚合根
/// </summary>
public class McpServer : AggregateRoot<Guid>
{
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 服务器类型（stdio 或 sse）
    /// </summary>
    public McpServerType ServerType { get; private set; }

    /// <summary>
    /// 执行命令（stdio 类型使用）
    /// </summary>
    public string? Command { get; private set; }

    /// <summary>
    /// 命令参数（JSON 数组格式）
    /// </summary>
    public string? Args { get; private set; }

    /// <summary>
    /// SSE URL（sse 类型使用）
    /// </summary>
    public string? SseUrl { get; private set; }

    /// <summary>
    /// 环境变量（JSON 对象格式）
    /// </summary>
    public string? Environment { get; private set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; private set; } = true;

    /// <summary>
    /// 描述
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // 导航属性
    private readonly List<McpTool> _tools = new();
    public IReadOnlyCollection<McpTool> Tools => _tools.AsReadOnly();

    private McpServer() { }

    public McpServer(Guid id, string name, McpServerType serverType) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        Name = name;
        ServerType = serverType;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateInfo(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        Name = name;
        Description = description ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetStdioConfig(string command, string? args, string? environment)
    {
        if (ServerType != McpServerType.Stdio)
            throw new InvalidOperationException("Cannot set stdio config for non-stdio server.");

        Command = command;
        Args = args;
        Environment = environment;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetSseConfig(string sseUrl, string? environment)
    {
        if (ServerType != McpServerType.Sse)
            throw new InvalidOperationException("Cannot set SSE config for non-SSE server.");

        SseUrl = sseUrl;
        Environment = environment;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetEnabled(bool enabled)
    {
        IsEnabled = enabled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddTool(McpTool tool)
    {
        if (!_tools.Any(t => t.Name == tool.Name))
        {
            _tools.Add(tool);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void RemoveTool(Guid toolId)
    {
        var tool = _tools.FirstOrDefault(t => t.Id == toolId);
        if (tool != null)
        {
            _tools.Remove(tool);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void ClearTools()
    {
        _tools.Clear();
        UpdatedAt = DateTime.UtcNow;
    }
}
