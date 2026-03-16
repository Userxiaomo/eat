namespace AiChat.Domain.Aggregates.BotAggregate;

public interface IBotRepository
{
    Task<Bot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Bot>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取用户可见的 Bot 列表（系统预设 + 公开的 + 自己创建的）
    /// </summary>
    Task<IEnumerable<Bot>> GetVisibleBotsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取系统预设的 Bot 列表
    /// </summary>
    Task<IEnumerable<Bot>> GetSystemBotsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取用户创建的 Bot 列表
    /// </summary>
    Task<IEnumerable<Bot>> GetUserBotsAsync(Guid userId, CancellationToken cancellationToken = default);

    Task AddAsync(Bot bot, CancellationToken cancellationToken = default);
    void Update(Bot bot);
    void Delete(Bot bot);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
