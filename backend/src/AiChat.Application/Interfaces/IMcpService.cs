using AiChat.Application.DTOs;

namespace AiChat.Application.Interfaces;

public interface IMcpService
{
    Task<IEnumerable<McpServerDto>> GetAllServersAsync(CancellationToken cancellationToken = default);
    Task<McpServerDto?> GetServerByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<McpServerDto> CreateServerAsync(CreateMcpServerRequest request, CancellationToken cancellationToken = default);
    Task<McpServerDto?> UpdateServerAsync(Guid id, UpdateMcpServerRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteServerAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<McpServerDto>> GetEnabledServersAsync(CancellationToken cancellationToken = default);
}
