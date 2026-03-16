using AiChat.Domain.Common;

namespace AiChat.Domain.Aggregates.BotAggregate;

/// <summary>
/// AI 智能体（Bot）聚合根
/// 支持预设和自定义 Prompt、头像，以及关联特定模型
/// </summary>
public class Bot : AggregateRoot<Guid>
{
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Bot 描述
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// 系统提示词（System Prompt）
    /// </summary>
    public string SystemPrompt { get; private set; } = string.Empty;

    /// <summary>
    /// 头像 URL
    /// </summary>
    public string? AvatarUrl { get; private set; }

    /// <summary>
    /// 关联的模型 ID（可选，不指定则使用对话默认模型）
    /// </summary>
    public Guid? ModelId { get; private set; }

    /// <summary>
    /// 是否为系统预设 Bot
    /// </summary>
    public bool IsSystem { get; private set; }

    /// <summary>
    /// 创建者用户 ID（系统预设 Bot 此值为空）
    /// </summary>
    public Guid? CreatedByUserId { get; private set; }

    /// <summary>
    /// 是否公开（其他用户可见）
    /// </summary>
    public bool IsPublic { get; private set; }

    /// <summary>
    /// 是否启用联网搜索
    /// </summary>
    public bool EnableWebSearch { get; private set; }

    /// <summary>
    /// 排序顺序
    /// </summary>
    public int SortOrder { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Bot() { }

    public Bot(Guid id, string name, string systemPrompt, bool isSystem = false, Guid? createdByUserId = null)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Bot name cannot be empty.", nameof(name));

        Name = name;
        SystemPrompt = systemPrompt;
        IsSystem = isSystem;
        CreatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateInfo(string name, string description, string systemPrompt)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Bot name cannot be empty.", nameof(name));

        Name = name;
        Description = description ?? string.Empty;
        SystemPrompt = systemPrompt ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetAvatar(string? avatarUrl)
    {
        AvatarUrl = avatarUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetModel(Guid? modelId)
    {
        ModelId = modelId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetPublic(bool isPublic)
    {
        IsPublic = isPublic;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetWebSearch(bool enable)
    {
        EnableWebSearch = enable;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetSortOrder(int order)
    {
        SortOrder = order;
        UpdatedAt = DateTime.UtcNow;
    }
}
