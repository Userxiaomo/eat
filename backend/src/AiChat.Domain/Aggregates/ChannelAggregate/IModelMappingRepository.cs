namespace AiChat.Domain.Aggregates.ChannelAggregate;

/// <summary>
/// 模型映射仓储接口
/// </summary>
public interface IModelMappingRepository
{
    Task<ModelMapping?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ModelMapping>> GetByChannelIdAsync(Guid channelId, CancellationToken cancellationToken = default);
    Task<ModelMapping?> GetByChannelAndFromModelAsync(Guid channelId, string fromModel, CancellationToken cancellationToken = default);
    Task<IEnumerable<ModelMapping>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(ModelMapping mapping, CancellationToken cancellationToken = default);
    void Update(ModelMapping mapping);
    void Delete(ModelMapping mapping);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
