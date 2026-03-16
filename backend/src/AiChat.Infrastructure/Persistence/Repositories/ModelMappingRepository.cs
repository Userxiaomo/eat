using AiChat.Domain.Aggregates.ChannelAggregate;
using AiChat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AiChat.Infrastructure.Persistence.Repositories;

public class ModelMappingRepository : IModelMappingRepository
{
    private readonly AiChatDbContext _context;

    public ModelMappingRepository(AiChatDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<ModelMapping?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<ModelMapping>()
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<ModelMapping>> GetByChannelIdAsync(Guid channelId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<ModelMapping>()
            .Where(m => m.ChannelId == channelId)
            .ToListAsync(cancellationToken);
    }

    public async Task<ModelMapping?> GetByChannelAndFromModelAsync(Guid channelId, string fromModel, CancellationToken cancellationToken = default)
    {
        return await _context.Set<ModelMapping>()
            .FirstOrDefaultAsync(m => m.ChannelId == channelId && m.FromModel == fromModel, cancellationToken);
    }

    public async Task<IEnumerable<ModelMapping>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<ModelMapping>().ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ModelMapping mapping, CancellationToken cancellationToken = default)
    {
        await _context.Set<ModelMapping>().AddAsync(mapping, cancellationToken);
    }

    public void Update(ModelMapping mapping)
    {
        _context.Set<ModelMapping>().Update(mapping);
    }

    public void Delete(ModelMapping mapping)
    {
        _context.Set<ModelMapping>().Remove(mapping);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
