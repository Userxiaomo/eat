using AiChat.Domain.Common;

namespace AiChat.Domain.Aggregates.UserAggregate;

/// <summary>
/// Token 限额类型
/// </summary>
public enum TokenLimitType
{
    /// <summary>
    /// 无限制
    /// </summary>
    Unlimited = 0,

    /// <summary>
    /// 按月限制
    /// </summary>
    Limited = 1
}

/// <summary>
/// 模型权限类型
/// </summary>
public enum GroupModelType
{
    /// <summary>
    /// 可使用所有模型
    /// </summary>
    All = 0,

    /// <summary>
    /// 仅可使用指定模型
    /// </summary>
    Specific = 1
}

/// <summary>
/// 用户分组聚合根
/// </summary>
public class Group : AggregateRoot<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public GroupModelType ModelType { get; private set; } = GroupModelType.All;
    public TokenLimitType TokenLimitType { get; private set; } = TokenLimitType.Unlimited;

    /// <summary>
    /// 每月 Token 限额（仅当 TokenLimitType = Limited 时有效）
    /// </summary>
    public int? MonthlyTokenLimit { get; private set; }

    /// <summary>
    /// 是否为默认分组（新用户自动加入）
    /// </summary>
    public bool IsDefault { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // 导航属性
    private readonly List<GroupModelMapping> _allowedModels = new();
    public IReadOnlyCollection<GroupModelMapping> AllowedModels => _allowedModels.AsReadOnly();

    private Group() { }

    public Group(Guid id, string name, bool isDefault = false) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Group name cannot be empty.", nameof(name));

        Name = name;
        IsDefault = isDefault;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Group name cannot be empty.", nameof(name));

        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetModelType(GroupModelType modelType)
    {
        ModelType = modelType;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetTokenLimit(TokenLimitType limitType, int? monthlyLimit = null)
    {
        TokenLimitType = limitType;
        MonthlyTokenLimit = limitType == TokenLimitType.Limited ? monthlyLimit : null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDefault(bool isDefault)
    {
        IsDefault = isDefault;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddAllowedModel(Guid modelId)
    {
        if (!_allowedModels.Any(m => m.ModelId == modelId))
        {
            _allowedModels.Add(new GroupModelMapping(Id, modelId));
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void RemoveAllowedModel(Guid modelId)
    {
        var mapping = _allowedModels.FirstOrDefault(m => m.ModelId == modelId);
        if (mapping != null)
        {
            _allowedModels.Remove(mapping);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void ClearAllowedModels()
    {
        _allowedModels.Clear();
        UpdatedAt = DateTime.UtcNow;
    }
}
