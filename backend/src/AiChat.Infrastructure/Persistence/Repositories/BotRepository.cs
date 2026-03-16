using AiChat.Domain.Aggregates.BotAggregate;
using Microsoft.EntityFrameworkCore;

namespace AiChat.Infrastructure.Persistence.Repositories;

public class BotRepository : IBotRepository
{
    private readonly AiChatDbContext _context;

    public BotRepository(AiChatDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Bot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Bots.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<Bot>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Bots.OrderBy(b => b.SortOrder).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Bot>> GetVisibleBotsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Bots
            .Where(b => b.IsSystem || b.IsPublic || b.CreatedByUserId == userId)
            .OrderBy(b => b.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Bot>> GetSystemBotsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Bots
            .Where(b => b.IsSystem)
            .OrderBy(b => b.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Bot>> GetUserBotsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Bots
            .Where(b => b.CreatedByUserId == userId)
            .OrderBy(b => b.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Bot bot, CancellationToken cancellationToken = default)
    {
        await _context.Bots.AddAsync(bot, cancellationToken);
    }

    public void Update(Bot bot)
    {
        _context.Bots.Update(bot);
    }

    public void Delete(Bot bot)
    {
        _context.Bots.Remove(bot);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
