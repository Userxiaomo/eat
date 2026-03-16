using AiChat.Domain.Aggregates.ChannelAggregate;
using Microsoft.EntityFrameworkCore;

namespace AiChat.Infrastructure.Persistence.Repositories;

public class AiModelRepository : IAiModelRepository
{
    private readonly AiChatDbContext _context;

    public AiModelRepository(AiChatDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<AiModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AiModels.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<AiModel>> GetByChannelIdAsync(Guid channelId, CancellationToken cancellationToken = default)
    {
        return await _context.AiModels
            .Where(m => m.ChannelId == channelId)
            .OrderBy(m => m.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AiModel>> GetAllEnabledAsync(ModelType? modelType = null, CancellationToken cancellationToken = default)
    {
        var query = _context.AiModels.Where(m => m.IsEnabled);

        if (modelType.HasValue)
            query = query.Where(m => m.ModelType == modelType.Value);

        return await query.OrderBy(m => m.SortOrder).ToListAsync(cancellationToken);
    }

    public async Task<AiModel> AddAsync(AiModel model, CancellationToken cancellationToken = default)
    {
        await _context.AiModels.AddAsync(model, cancellationToken);
        return model;
    }

    public void Update(AiModel model)
    {
        _context.AiModels.Update(model);
    }

    public void Delete(AiModel model)
    {
        _context.AiModels.Remove(model);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
