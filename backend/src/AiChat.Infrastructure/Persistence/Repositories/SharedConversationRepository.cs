using AiChat.Domain.Aggregates.ConversationAggregate;
using AiChat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AiChat.Infrastructure.Persistence.Repositories;

public class SharedConversationRepository : ISharedConversationRepository
{
    private readonly AiChatDbContext _context;

    public SharedConversationRepository(AiChatDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<SharedConversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<SharedConversation>()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<SharedConversation?> GetByShareHashAsync(string shareHash, CancellationToken cancellationToken = default)
    {
        return await _context.Set<SharedConversation>()
            .FirstOrDefaultAsync(s => s.ShareHash == shareHash, cancellationToken);
    }

    public async Task<IEnumerable<SharedConversation>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<SharedConversation>()
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<SharedConversation>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<SharedConversation>().ToListAsync(cancellationToken);
    }

    public async Task AddAsync(SharedConversation sharedConversation, CancellationToken cancellationToken = default)
    {
        await _context.Set<SharedConversation>().AddAsync(sharedConversation, cancellationToken);
    }

    public void Update(SharedConversation sharedConversation)
    {
        _context.Set<SharedConversation>().Update(sharedConversation);
    }

    public void Delete(SharedConversation sharedConversation)
    {
        _context.Set<SharedConversation>().Remove(sharedConversation);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
