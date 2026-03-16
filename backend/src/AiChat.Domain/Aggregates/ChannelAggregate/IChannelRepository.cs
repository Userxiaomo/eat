namespace AiChat.Domain.Aggregates.ChannelAggregate;

public interface IChannelRepository
{
    Task<Channel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Channel>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Channel> AddAsync(Channel channel, CancellationToken cancellationToken = default);
    void Update(Channel channel);
    void Delete(Channel channel);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
