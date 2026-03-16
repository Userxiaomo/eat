using AiChat.Domain.Common;

namespace AiChat.Domain.Aggregates.UserAggregate;

/// <summary>
/// 用户角色
/// </summary>
public enum UserRole
{
    User = 0,
    Admin = 1
}

/// <summary>
/// 用户聚合根
/// </summary>
public class User : AggregateRoot<Guid>
{
    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; } = UserRole.User;
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// 用户所属分组 ID
    /// </summary>
    public Guid? GroupId { get; private set; }

    /// <summary>
    /// 当月累计使用 Token 数量
    /// </summary>
    public int CurrentMonthTotalTokens { get; private set; }

    /// <summary>
    /// Token 统计最后更新时间
    /// </summary>
    public DateTime UsageUpdatedAt { get; private set; }

    // === 用户管理字段 ===
    /// <summary>
    /// 是否被封禁
    /// </summary>
    public bool IsBanned { get; private set; } = false;

    /// <summary>
    /// 封禁时间
    /// </summary>
    public DateTime? BannedAt { get; private set; }

    /// <summary>
    /// 封禁原因
    /// </summary>
    public string? BanReason { get; private set; }

    // EF Core需要的无参构造函数
    private User()
    {
    }

    public User(Guid id, string username, string email, string passwordHash, UserRole role = UserRole.User)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be empty.", nameof(username));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));

        Username = username;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        CreatedAt = DateTime.UtcNow;
        UsageUpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("Password hash cannot be empty.", nameof(newPasswordHash));

        PasswordHash = newPasswordHash;
    }

    public void SetRole(UserRole role)
    {
        Role = role;
    }

    public void SetGroup(Guid? groupId)
    {
        GroupId = groupId;
    }

    /// <summary>
    /// 增加 Token 使用量
    /// </summary>
    public void AddTokenUsage(int tokens)
    {
        // 如果是新的月份，重置计数
        if (UsageUpdatedAt.Year != DateTime.UtcNow.Year || UsageUpdatedAt.Month != DateTime.UtcNow.Month)
        {
            CurrentMonthTotalTokens = 0;
        }

        CurrentMonthTotalTokens += tokens;
        UsageUpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 重置当月 Token 使用量
    /// </summary>
    public void ResetMonthlyTokenUsage()
    {
        CurrentMonthTotalTokens = 0;
        UsageUpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 封禁用户
    /// </summary>
    public void Ban(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Ban reason cannot be empty.", nameof(reason));

        IsBanned = true;
        BannedAt = DateTime.UtcNow;
        BanReason = reason;
    }

    /// <summary>
    /// 解封用户
    /// </summary>
    public void Unban()
    {
        IsBanned = false;
        BannedAt = null;
        BanReason = null;
    }
}

