namespace AiChat.Domain.Aggregates.ConversationAggregate;

/// <summary>
/// 分享对话仓储接口
/// </summary>
public interface ISharedConversationRepository
{
    Task<SharedConversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SharedConversation?> GetByShareHashAsync(string shareHash, CancellationToken cancellationToken = default);
    Task<IEnumerable<SharedConversation>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<SharedConversation>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(SharedConversation sharedConversation, CancellationToken cancellationToken = default);
    void Update(SharedConversation sharedConversation);
    void Delete(SharedConversation sharedConversation);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
