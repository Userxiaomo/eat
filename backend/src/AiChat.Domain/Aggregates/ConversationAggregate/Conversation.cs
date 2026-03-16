using AiChat.Domain.Common;
using AiChat.Domain.Exceptions;

namespace AiChat.Domain.Aggregates.ConversationAggregate;

/// <summary>
/// 历史消息策略类型
/// </summary>
public enum HistoryType
{
    /// <summary>
    /// 保留所有历史消息
    /// </summary>
    All = 0,

    /// <summary>
    /// 保留指定数量的历史消息
    /// </summary>
    Count = 1,

    /// <summary>
    /// 不保留历史消息
    /// </summary>
    None = 2
}

/// <summary>
/// 头像类型
/// </summary>
public enum AvatarType
{
    None = 0,
    Emoji = 1,
    Url = 2
}

/// <summary>
/// 会话聚合根
/// </summary>
public class Conversation : AggregateRoot<Guid>
{
    private readonly List<Message> _messages = new();

    public string Title { get; private set; } = string.Empty;
    public Guid UserId { get; private set; }
    public string ModelId { get; private set; } = string.Empty;
    public ConversationSettings Settings { get; private set; } = ConversationSettings.Default;
    public IReadOnlyCollection<Message> Messages => _messages.AsReadOnly();
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // === Token 统计 ===
    /// <summary>
    /// 累计输入 Token 数量
    /// </summary>
    public int InputTokens { get; private set; }

    /// <summary>
    /// 累计输出 Token 数量
    /// </summary>
    public int OutputTokens { get; private set; }

    /// <summary>
    /// 累计总 Token 数量
    /// </summary>
    public int TotalTokens => InputTokens + OutputTokens;

    // === 历史消息策略 ===
    /// <summary>
    /// 历史消息策略类型
    /// </summary>
    public HistoryType HistoryType { get; private set; } = HistoryType.Count;

    /// <summary>
    /// 保留的历史消息数量（仅当 HistoryType = Count 时有效）
    /// </summary>
    public int HistoryCount { get; private set; } = 5;

    // === 收藏功能 ===
    /// <summary>
    /// 是否已收藏
    /// </summary>
    public bool IsStar { get; private set; }

    /// <summary>
    /// 收藏时间
    /// </summary>
    public DateTime? StarAt { get; private set; }

    // === 搜索功能 ===
    /// <summary>
    /// 是否启用 Web 搜索
    /// </summary>
    public bool SearchEnabled { get; private set; }

    // === 头像 ===
    /// <summary>
    /// 头像（Emoji 或 URL）
    /// </summary>
    public string? Avatar { get; private set; }

    /// <summary>
    /// 头像类型
    /// </summary>
    public AvatarType AvatarType { get; private set; } = AvatarType.None;

    // === Bot 关联 ===
    /// <summary>
    /// 关联的 Bot ID
    /// </summary>
    public Guid? BotId { get; private set; }

    /// <summary>
    /// 是否为 Bot 会话
    /// </summary>
    public bool IsWithBot { get; private set; }

    /// <summary>
    /// 默认 Provider ID（渠道 ID）
    /// </summary>
    public Guid? DefaultProviderId { get; private set; }

    // EF Core需要的无参构造函数
    private Conversation()
    {
    }

    public Conversation(Guid id, string title, Guid userId, string modelId, ConversationSettings? settings = null)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Conversation title cannot be empty.", nameof(title));

        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty.", nameof(userId));

        if (string.IsNullOrWhiteSpace(modelId))
            throw new ArgumentException("ModelId cannot be empty.", nameof(modelId));

        Title = title;
        UserId = userId;
        ModelId = modelId;
        Settings = settings ?? ConversationSettings.Default;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 添加消息
    /// </summary>
    public Message AddMessage(string content, MessageRole role)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new DomainException("Message content cannot be empty.");

        var message = new Message(Guid.NewGuid(), content, role, Id);
        _messages.Add(message);
        UpdatedAt = DateTime.UtcNow;

        return message;
    }

    /// <summary>
    /// 添加消息（带思维链和 Token 统计）
    /// </summary>
    public Message AddMessage(string content, MessageRole role, string? reasoningContent, int? inputTokens = null, int? outputTokens = null)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new DomainException("Message content cannot be empty.");

        var message = new Message(Guid.NewGuid(), content, role, Id, reasoningContent);
        message.SetTokenUsage(inputTokens, outputTokens);
        _messages.Add(message);
        UpdatedAt = DateTime.UtcNow;

        return message;
    }

    /// <summary>
    /// 更新标题
    /// </summary>
    public void UpdateTitle(string newTitle)
    {
        if (string.IsNullOrWhiteSpace(newTitle))
            throw new DomainException("Conversation title cannot be empty.");

        Title = newTitle;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 切换AI模型
    /// </summary>
    public void ChangeModel(string newModelId)
    {
        if (string.IsNullOrWhiteSpace(newModelId))
            throw new DomainException("ModelId cannot be empty.");

        ModelId = newModelId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 更新设置
    /// </summary>
    public void UpdateSettings(ConversationSettings newSettings)
    {
        Settings = newSettings ?? throw new ArgumentNullException(nameof(newSettings));
        UpdatedAt = DateTime.UtcNow;
    }

    // === Token 统计相关方法 ===
    /// <summary>
    /// 增加 Token 使用量
    /// </summary>
    public void AddTokenUsage(int inputTokens, int outputTokens)
    {
        InputTokens += inputTokens;
        OutputTokens += outputTokens;
        UpdatedAt = DateTime.UtcNow;
    }

    // === 收藏相关方法 ===
    /// <summary>
    /// 收藏会话
    /// </summary>
    public void Star()
    {
        IsStar = true;
        StarAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 取消收藏
    /// </summary>
    public void Unstar()
    {
        IsStar = false;
        StarAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    // === 历史消息策略相关方法 ===
    /// <summary>
    /// 设置历史消息策略
    /// </summary>
    public void SetHistoryType(HistoryType type, int count = 5)
    {
        HistoryType = type;
        HistoryCount = type == HistoryType.Count ? Math.Max(1, count) : 0;
        UpdatedAt = DateTime.UtcNow;
    }

    // === 搜索开关 ===
    /// <summary>
    /// 启用/禁用 Web 搜索
    /// </summary>
    public void SetSearchEnabled(bool enabled)
    {
        SearchEnabled = enabled;
        UpdatedAt = DateTime.UtcNow;
    }

    // === 头像相关方法 ===
    /// <summary>
    /// 设置头像
    /// </summary>
    public void SetAvatar(string? avatar, AvatarType type)
    {
        Avatar = avatar;
        AvatarType = type;
        UpdatedAt = DateTime.UtcNow;
    }

    // === Bot 关联 ===
    /// <summary>
    /// 关联 Bot
    /// </summary>
    public void SetBot(Guid? botId)
    {
        BotId = botId;
        IsWithBot = botId.HasValue;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 设置默认 Provider
    /// </summary>
    public void SetDefaultProvider(Guid? providerId)
    {
        DefaultProviderId = providerId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 软删除消息
    /// </summary>
    public void SoftDeleteMessage(Guid messageId)
    {
        var message = _messages.FirstOrDefault(m => m.Id == messageId);
        if (message != null)
        {
            message.SoftDelete();
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
