namespace AiChat.Domain.Aggregates.UserAggregate;

/// <summary>
/// Refresh Token 仓储接口
/// </summary>
public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<IEnumerable<RefreshToken>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    void Update(RefreshToken refreshToken);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
