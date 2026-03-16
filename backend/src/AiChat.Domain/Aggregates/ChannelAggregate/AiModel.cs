using AiChat.Domain.Common;

namespace AiChat.Domain.Aggregates.ChannelAggregate;

/// <summary>
/// AI 模型实体
/// </summary>
public class AiModel : Entity<Guid>
{
    public Guid ChannelId { get; private set; }
    public string Name { get; private set; } = string.Empty;        // 显示名称
    public string ModelId { get; private set; } = string.Empty;     // 模型 ID（如 gpt-4）
    public ModelType ModelType { get; private set; }
    public int MaxTokens { get; private set; } = 4096;
    public decimal InputPrice { get; private set; }                 // 输入价格（每千 Token）
    public decimal OutputPrice { get; private set; }                // 输出价格（每千 Token）
    public bool IsEnabled { get; private set; } = true;
    public int SortOrder { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // === 模型能力标识 ===
    /// <summary>
    /// 是否支持视觉理解（图片输入）
    /// </summary>
    public bool SupportVision { get; private set; }

    /// <summary>
    /// 是否支持工具调用（Function Calling）
    /// </summary>
    public bool SupportTool { get; private set; }

    /// <summary>
    /// 是否有内置图像生成能力
    /// </summary>
    public bool BuiltInImageGen { get; private set; }

    /// <summary>
    /// 是否有内置 Web 搜索能力
    /// </summary>
    public bool BuiltInWebSearch { get; private set; }

    /// <summary>
    /// 是否被选中（用户可见）
    /// </summary>
    public bool Selected { get; private set; } = true;

    private AiModel() { }

    public AiModel(
        Guid id,
        Guid channelId,
        string name,
        string modelId,
        ModelType modelType,
        int maxTokens = 4096,
        decimal inputPrice = 0,
        decimal outputPrice = 0,
        int sortOrder = 0)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(modelId))
            throw new ArgumentException("ModelId cannot be empty.", nameof(modelId));

        ChannelId = channelId;
        Name = name;
        ModelId = modelId;
        ModelType = modelType;
        MaxTokens = maxTokens;
        InputPrice = inputPrice;
        OutputPrice = outputPrice;
        SortOrder = sortOrder;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(
        string name,
        string modelId,
        ModelType modelType,
        int maxTokens,
        decimal inputPrice,
        decimal outputPrice,
        int sortOrder)
    {
        Name = name;
        ModelId = modelId;
        ModelType = modelType;
        MaxTokens = maxTokens;
        InputPrice = inputPrice;
        OutputPrice = outputPrice;
        SortOrder = sortOrder;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetEnabled(bool enabled)
    {
        IsEnabled = enabled;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 设置模型能力标识
    /// </summary>
    public void SetCapabilities(bool supportVision, bool supportTool, bool builtInImageGen, bool builtInWebSearch)
    {
        SupportVision = supportVision;
        SupportTool = supportTool;
        BuiltInImageGen = builtInImageGen;
        BuiltInWebSearch = builtInWebSearch;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 设置是否选中
    /// </summary>
    public void SetSelected(bool selected)
    {
        Selected = selected;
        UpdatedAt = DateTime.UtcNow;
    }
}
