using AiChat.Domain.Common;

namespace AiChat.Domain.Aggregates.UserAggregate;

/// <summary>
/// Refresh Token 实体
/// 用于实现 JWT Token 刷新机制
/// </summary>
public class RefreshToken : Entity<Guid>
{
    /// <summary>
    /// Token 值（使用安全随机生成）
    /// </summary>
    public string Token { get; private set; } = string.Empty;

    /// <summary>
    /// 关联的用户ID
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime ExpiresAt { get; private set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// 是否已撤销
    /// </summary>
    public bool IsRevoked { get; private set; }

    /// <summary>
    /// 撤销时间
    /// </summary>
    public DateTime? RevokedAt { get; private set; }

    /// <summary>
    /// 替代此 Token 的新 Token ID（用于 Token 轮换）
    /// </summary>
    public Guid? ReplacedByTokenId { get; private set; }

    // EF Core 需要的无参构造函数
    private RefreshToken() { }

    public RefreshToken(Guid id, string token, Guid userId, DateTime expiresAt)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be empty.", nameof(token));

        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty.", nameof(userId));

        Token = token;
        UserId = userId;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
        IsRevoked = false;
    }

    /// <summary>
    /// 检查 Token 是否有效（未过期且未撤销）
    /// </summary>
    public bool IsActive => !IsRevoked && DateTime.UtcNow < ExpiresAt;

    /// <summary>
    /// 撤销此 Token
    /// </summary>
    public void Revoke(Guid? replacedByTokenId = null)
    {
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
        ReplacedByTokenId = replacedByTokenId;
    }
}
