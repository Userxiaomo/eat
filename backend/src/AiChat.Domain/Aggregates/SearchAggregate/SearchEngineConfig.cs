using AiChat.Domain.Common;

namespace AiChat.Domain.Aggregates.SearchAggregate;

/// <summary>
/// 搜索引擎类型
/// </summary>
public enum SearchEngineType
{
    Tavily = 0,
    Jina = 1,
    Bocha = 2,
    Bing = 3,
    Google = 4,
    Custom = 99
}

/// <summary>
/// 搜索引擎配置实体
/// </summary>
public class SearchEngineConfig : Entity<Guid>
{
    public SearchEngineType EngineType { get; private set; }

    /// <summary>
    /// 显示名称
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// API Key（加密存储）
    /// </summary>
    public string ApiKey { get; private set; } = string.Empty;

    /// <summary>
    /// API URL（用于自定义搜索引擎）
    /// </summary>
    public string? ApiUrl { get; private set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; private set; }

    /// <summary>
    /// 是否为默认搜索引擎
    /// </summary>
    public bool IsDefault { get; private set; }

    /// <summary>
    /// 最大返回结果数
    /// </summary>
    public int MaxResults { get; private set; } = 5;

    /// <summary>
    /// 是否启用关键词提取
    /// </summary>
    public bool ExtractKeywords { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private SearchEngineConfig() { }

    public SearchEngineConfig(Guid id, SearchEngineType engineType, string name, string apiKey)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        EngineType = engineType;
        Name = name;
        ApiKey = apiKey;
        IsEnabled = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateConfig(string name, string apiKey, string? apiUrl)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        Name = name;
        ApiKey = apiKey;
        ApiUrl = apiUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetEnabled(bool enabled)
    {
        IsEnabled = enabled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDefault(bool isDefault)
    {
        IsDefault = isDefault;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 设置搜索选项
    /// </summary>
    public void SetSearchOptions(int maxResults, bool extractKeywords)
    {
        MaxResults = Math.Max(1, Math.Min(maxResults, 20));
        ExtractKeywords = extractKeywords;
        UpdatedAt = DateTime.UtcNow;
    }
}
