using AiChat.Domain.Aggregates.SearchAggregate;
using Microsoft.EntityFrameworkCore;

namespace AiChat.Infrastructure.Persistence.Repositories;

public class SearchEngineConfigRepository : ISearchEngineConfigRepository
{
    private readonly AiChatDbContext _context;

    public SearchEngineConfigRepository(AiChatDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<SearchEngineConfig?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.SearchEngineConfigs.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<SearchEngineConfig>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SearchEngineConfigs.ToListAsync(cancellationToken);
    }

    public async Task<SearchEngineConfig?> GetDefaultAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SearchEngineConfigs
            .FirstOrDefaultAsync(s => s.IsDefault && s.IsEnabled, cancellationToken);
    }

    public async Task<SearchEngineConfig?> GetEnabledByTypeAsync(SearchEngineType type, CancellationToken cancellationToken = default)
    {
        return await _context.SearchEngineConfigs
            .FirstOrDefaultAsync(s => s.EngineType == type && s.IsEnabled, cancellationToken);
    }

    public async Task<SearchEngineConfig?> GetFirstEnabledAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SearchEngineConfigs
            .FirstOrDefaultAsync(s => s.IsEnabled, cancellationToken);
    }

    public async Task AddAsync(SearchEngineConfig config, CancellationToken cancellationToken = default)
    {
        await _context.SearchEngineConfigs.AddAsync(config, cancellationToken);
    }

    public void Update(SearchEngineConfig config)
    {
        _context.SearchEngineConfigs.Update(config);
    }

    public void Delete(SearchEngineConfig config)
    {
        _context.SearchEngineConfigs.Remove(config);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
