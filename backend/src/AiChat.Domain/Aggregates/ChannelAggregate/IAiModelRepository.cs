namespace AiChat.Domain.Aggregates.ChannelAggregate;

public interface IAiModelRepository
{
    Task<AiModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<AiModel>> GetByChannelIdAsync(Guid channelId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AiModel>> GetAllEnabledAsync(ModelType? modelType = null, CancellationToken cancellationToken = default);
    Task<AiModel> AddAsync(AiModel model, CancellationToken cancellationToken = default);
    void Update(AiModel model);
    void Delete(AiModel model);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
