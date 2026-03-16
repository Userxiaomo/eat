using AiChat.Domain.Aggregates.UserAggregate;

namespace AiChat.Application.Interfaces;

public interface IJwtTokenGenerator
{
    /// <summary>
    /// 生成 Access Token
    /// </summary>
    (string Token, DateTime ExpiresAt) GenerateToken(Guid userId, string username, UserRole role);

    /// <summary>
    /// 生成 Refresh Token
    /// </summary>
    string GenerateRefreshToken();
}
