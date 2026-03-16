using AiChat.Domain.Aggregates.UserAggregate;
using Microsoft.EntityFrameworkCore;

namespace AiChat.Infrastructure.Persistence.Repositories;

public class GroupRepository : IGroupRepository
{
    private readonly AiChatDbContext _context;

    public GroupRepository(AiChatDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Group?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Groups
            .Include(g => g.AllowedModels)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Group>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Groups
            .Include(g => g.AllowedModels)
            .ToListAsync(cancellationToken);
    }

    public async Task<Group?> GetDefaultGroupAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Groups
            .Include(g => g.AllowedModels)
            .FirstOrDefaultAsync(g => g.IsDefault, cancellationToken);
    }

    public async Task AddAsync(Group group, CancellationToken cancellationToken = default)
    {
        await _context.Groups.AddAsync(group, cancellationToken);
    }

    public void Update(Group group)
    {
        _context.Groups.Update(group);
    }

    public void Delete(Group group)
    {
        _context.Groups.Remove(group);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
