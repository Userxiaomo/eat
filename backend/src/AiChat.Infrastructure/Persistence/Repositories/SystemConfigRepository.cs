using AiChat.Domain.Aggregates.SystemAggregate;
using AiChat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AiChat.Infrastructure.Persistence.Repositories;

public class SystemConfigRepository : ISystemConfigRepository
{
    private readonly AiChatDbContext _context;

    public SystemConfigRepository(AiChatDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<SystemConfiguration?> GetAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<SystemConfiguration>().FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<SystemConfiguration> GetOrCreateAsync(CancellationToken cancellationToken = default)
    {
        var config = await GetAsync(cancellationToken);
        if (config != null)
            return config;

        // 创建默认配置
        config = new SystemConfiguration(Guid.NewGuid());
        await _context.Set<SystemConfiguration>().AddAsync(config, cancellationToken);
        await SaveChangesAsync(cancellationToken);

        return config;
    }

    public void Update(SystemConfiguration config)
    {
        _context.Set<SystemConfiguration>().Update(config);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
