namespace AiChat.Domain.Aggregates.UserAggregate;

public interface IGroupRepository
{
    Task<Group?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Group>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Group?> GetDefaultGroupAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Group group, CancellationToken cancellationToken = default);
    void Update(Group group);
    void Delete(Group group);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
