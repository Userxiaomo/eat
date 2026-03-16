namespace AiChat.Domain.Aggregates.McpAggregate;

public interface IMcpServerRepository
{
    Task<McpServer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<McpServer>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<McpServer>> GetEnabledServersAsync(CancellationToken cancellationToken = default);
    Task AddAsync(McpServer server, CancellationToken cancellationToken = default);
    void Update(McpServer server);
    void Delete(McpServer server);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
