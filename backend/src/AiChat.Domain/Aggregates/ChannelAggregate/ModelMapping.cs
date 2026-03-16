using AiChat.Domain.Common;

namespace AiChat.Domain.Aggregates.ChannelAggregate;

/// <summary>
/// 模型映射实体
/// 用于将前端展示的模型名映射到实际调用上游的模型名
/// </summary>
public class ModelMapping : Entity<Guid>
{
    public Guid ChannelId { get; private set; }

    /// <summary>
    /// 前端展示的模型名称
    /// </summary>
    public string FromModel { get; private set; } = string.Empty;

    /// <summary>
    /// 实际调用上游的模型名称
    /// </summary>
    public string ToModel { get; private set; } = string.Empty;

    /// <summary>
    /// 是否隐藏上游模型（加 ! 前缀的效果）
    /// 如果为 true，ToModel 不会出现在可用模型列表中
    /// </summary>
    public bool HideUpstream { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // EF Core 需要
    private ModelMapping() { }

    public ModelMapping(
        Guid id,
        Guid channelId,
        string fromModel,
        string toModel,
        bool hideUpstream = false)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(fromModel))
            throw new ArgumentException("FromModel cannot be empty.", nameof(fromModel));

        if (string.IsNullOrWhiteSpace(toModel))
            throw new ArgumentException("ToModel cannot be empty.", nameof(toModel));

        ChannelId = channelId;
        FromModel = fromModel;
        ToModel = toModel;
        HideUpstream = hideUpstream;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string fromModel, string toModel, bool hideUpstream)
    {
        if (string.IsNullOrWhiteSpace(fromModel))
            throw new ArgumentException("FromModel cannot be empty.", nameof(fromModel));

        if (string.IsNullOrWhiteSpace(toModel))
            throw new ArgumentException("ToModel cannot be empty.", nameof(toModel));

        FromModel = fromModel;
        ToModel = toModel;
        HideUpstream = hideUpstream;
        UpdatedAt = DateTime.UtcNow;
    }
}
