using AiChat.Domain.Aggregates.ChannelAggregate;
using Microsoft.EntityFrameworkCore;

namespace AiChat.Infrastructure.Persistence.Repositories;

public class ChannelRepository : IChannelRepository
{
    private readonly AiChatDbContext _context;

    public ChannelRepository(AiChatDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Channel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Channels
            .Include(c => c.Models)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Channel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Channels
            .Include(c => c.Models)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Channel> AddAsync(Channel channel, CancellationToken cancellationToken = default)
    {
        await _context.Channels.AddAsync(channel, cancellationToken);
        return channel;
    }

    public void Update(Channel channel)
    {
        _context.Channels.Update(channel);
    }

    public void Delete(Channel channel)
    {
        _context.Channels.Remove(channel);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
