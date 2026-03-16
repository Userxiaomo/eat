using AiChat.Domain.Aggregates.McpAggregate;
using Microsoft.EntityFrameworkCore;

namespace AiChat.Infrastructure.Persistence.Repositories;

public class McpServerRepository : IMcpServerRepository
{
    private readonly AiChatDbContext _context;

    public McpServerRepository(AiChatDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<McpServer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.McpServers
            .Include(s => s.Tools)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<McpServer>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.McpServers
            .Include(s => s.Tools)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<McpServer>> GetEnabledServersAsync(CancellationToken cancellationToken = default)
    {
        return await _context.McpServers
            .Include(s => s.Tools.Where(t => t.IsEnabled))
            .Where(s => s.IsEnabled)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(McpServer server, CancellationToken cancellationToken = default)
    {
        await _context.McpServers.AddAsync(server, cancellationToken);
    }

    public void Update(McpServer server)
    {
        _context.McpServers.Update(server);
    }

    public void Delete(McpServer server)
    {
        _context.McpServers.Remove(server);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
