namespace AiChat.Domain.Aggregates.SearchAggregate;

public interface ISearchEngineConfigRepository
{
    Task<SearchEngineConfig?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<SearchEngineConfig>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<SearchEngineConfig?> GetDefaultAsync(CancellationToken cancellationToken = default);
    Task<SearchEngineConfig?> GetEnabledByTypeAsync(SearchEngineType type, CancellationToken cancellationToken = default);
    Task<SearchEngineConfig?> GetFirstEnabledAsync(CancellationToken cancellationToken = default);
    Task AddAsync(SearchEngineConfig config, CancellationToken cancellationToken = default);
    void Update(SearchEngineConfig config);
    void Delete(SearchEngineConfig config);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
