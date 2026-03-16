namespace AiChat.Domain.Aggregates.ConversationAggregate;

/// <summary>
/// 会话仓储接口
/// </summary>
public interface IConversationRepository
{
    Task<Conversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Conversation>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 分页获取用户会话
    /// </summary>
    Task<(IEnumerable<Conversation> Items, int TotalCount)> GetByUserIdPagedAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<Conversation> AddAsync(Conversation conversation, CancellationToken cancellationToken = default);
    void Update(Conversation conversation);
    void Delete(Conversation conversation);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
