namespace AiChat.Domain.Aggregates.SystemAggregate;

/// <summary>
/// 系统配置仓储接口
/// </summary>
public interface ISystemConfigRepository
{
    Task<SystemConfiguration?> GetAsync(CancellationToken cancellationToken = default);
    Task<SystemConfiguration> GetOrCreateAsync(CancellationToken cancellationToken = default);
    void Update(SystemConfiguration config);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
