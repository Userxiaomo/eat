using AiChat.Domain.Common;

namespace AiChat.Domain.Aggregates.ChannelAggregate;

/// <summary>
/// 渠道类型（默认/自定义）
/// </summary>
public enum ChannelType
{
    Default = 0,
    Custom = 1
}

/// <summary>
/// AI 渠道聚合根
/// </summary>
public class Channel : AggregateRoot<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public Provider Provider { get; private set; }
    public string ApiKey { get; private set; } = string.Empty;  // 加密存储
    public string BaseUrl { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>
    /// API 风格（OpenAI/Claude/Gemini）
    /// </summary>
    public ApiStyle ApiStyle { get; private set; } = ApiStyle.OpenAI;

    /// <summary>
    /// 渠道类型
    /// </summary>
    public ChannelType ChannelType { get; private set; } = ChannelType.Default;

    /// <summary>
    /// 渠道 Logo URL
    /// </summary>
    public string? Logo { get; private set; }

    /// <summary>
    /// 排序顺序
    /// </summary>
    public int SortOrder { get; private set; }

    // === 负载均衡相关属性 ===
    /// <summary>
    /// 优先级（数字越小优先级越高）
    /// </summary>
    public int Priority { get; private set; } = 0;

    /// <summary>
    /// 权重（同优先级下的负载均衡权重，越大被选中概率越高）
    /// </summary>
    public int Weight { get; private set; } = 1;

    /// <summary>
    /// 失败时最大重试次数
    /// </summary>
    public int MaxRetries { get; private set; } = 1;

    /// <summary>
    /// 渠道健康状态
    /// </summary>
    public bool IsHealthy { get; private set; } = true;

    /// <summary>
    /// 最后失败时间（用于自动恢复判断）
    /// </summary>
    public DateTime? LastFailedAt { get; private set; }


    // 导航属性
    private readonly List<AiModel> _models = new();
    public IReadOnlyCollection<AiModel> Models => _models.AsReadOnly();

    private Channel() { }

    public Channel(Guid id, string name, Provider provider, string apiKey, string baseUrl, ApiStyle apiStyle = ApiStyle.OpenAI)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        Name = name;
        Provider = provider;
        ApiKey = apiKey;
        BaseUrl = baseUrl;
        ApiStyle = apiStyle;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string name, Provider provider, string? apiKey, string baseUrl, ApiStyle? apiStyle = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        Name = name;
        Provider = provider;
        if (!string.IsNullOrEmpty(apiKey))
            ApiKey = apiKey;
        BaseUrl = baseUrl;
        if (apiStyle.HasValue)
            ApiStyle = apiStyle.Value;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetEnabled(bool enabled)
    {
        IsEnabled = enabled;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 设置 Logo
    /// </summary>
    public void SetLogo(string? logo)
    {
        Logo = logo;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 设置排序顺序
    /// </summary>
    public void SetSortOrder(int order)
    {
        SortOrder = order;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 设置渠道类型
    /// </summary>
    public void SetChannelType(ChannelType type)
    {
        ChannelType = type;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddModel(AiModel model)
    {
        _models.Add(model);
    }

    // === 负载均衡方法 ===
    /// <summary>
    /// 设置优先级
    /// </summary>
    public void SetPriority(int priority)
    {
        if (priority < 0)
            throw new ArgumentException("Priority cannot be negative.", nameof(priority));

        Priority = priority;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 设置权重
    /// </summary>
    public void SetWeight(int weight)
    {
        if (weight < 1)
            throw new ArgumentException("Weight must be at least 1.", nameof(weight));

        Weight = weight;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 设置最大重试次数
    /// </summary>
    public void SetMaxRetries(int retries)
    {
        if (retries < 0)
            throw new ArgumentException("MaxRetries cannot be negative.", nameof(retries));

        MaxRetries = retries;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 标记渠道失败
    /// </summary>
    public void MarkFailed()
    {
        IsHealthy = false;
        LastFailedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 标记渠道健康
    /// </summary>
    public void MarkHealthy()
    {
        IsHealthy = true;
        LastFailedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

}
