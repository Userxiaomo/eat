using AiChat.Application.DTOs;
using AiChat.Application.Interfaces;
using AiChat.Domain.Aggregates.McpAggregate;

namespace AiChat.Application.Services;

public class McpService : IMcpService
{
    private readonly IMcpServerRepository _repository;

    public McpService(IMcpServerRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<IEnumerable<McpServerDto>> GetAllServersAsync(CancellationToken cancellationToken = default)
    {
        var servers = await _repository.GetAllAsync(cancellationToken);
        return servers.Select(MapToDto);
    }

    public async Task<McpServerDto?> GetServerByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var server = await _repository.GetByIdAsync(id, cancellationToken);
        return server == null ? null : MapToDto(server);
    }

    public async Task<McpServerDto> CreateServerAsync(CreateMcpServerRequest request, CancellationToken cancellationToken = default)
    {
        var serverType = (McpServerType)request.ServerType;
        var server = new McpServer(Guid.NewGuid(), request.Name, serverType);

        server.UpdateInfo(request.Name, request.Description);

        if (serverType == McpServerType.Stdio)
        {
            server.SetStdioConfig(request.Command ?? "", request.Args, request.Environment);
        }
        else if (serverType == McpServerType.Sse)
        {
            server.SetSseConfig(request.SseUrl ?? "", request.Environment);
        }

        await _repository.AddAsync(server, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return MapToDto(server);
    }

    public async Task<McpServerDto?> UpdateServerAsync(Guid id, UpdateMcpServerRequest request, CancellationToken cancellationToken = default)
    {
        var server = await _repository.GetByIdAsync(id, cancellationToken);
        if (server == null) return null;

        if (request.Name != null || request.Description != null)
        {
            server.UpdateInfo(request.Name ?? server.Name, request.Description ?? server.Description);
        }

        if (server.ServerType == McpServerType.Stdio &&
            (request.Command != null || request.Args != null || request.Environment != null))
        {
            server.SetStdioConfig(
                request.Command ?? server.Command ?? "",
                request.Args ?? server.Args,
                request.Environment ?? server.Environment);
        }

        if (server.ServerType == McpServerType.Sse &&
            (request.SseUrl != null || request.Environment != null))
        {
            server.SetSseConfig(
                request.SseUrl ?? server.SseUrl ?? "",
                request.Environment ?? server.Environment);
        }

        if (request.IsEnabled.HasValue)
        {
            server.SetEnabled(request.IsEnabled.Value);
        }

        _repository.Update(server);
        await _repository.SaveChangesAsync(cancellationToken);

        return MapToDto(server);
    }

    public async Task<bool> DeleteServerAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var server = await _repository.GetByIdAsync(id, cancellationToken);
        if (server == null) return false;

        _repository.Delete(server);
        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<IEnumerable<McpServerDto>> GetEnabledServersAsync(CancellationToken cancellationToken = default)
    {
        var servers = await _repository.GetEnabledServersAsync(cancellationToken);
        return servers.Select(MapToDto);
    }

    private static McpServerDto MapToDto(McpServer server) => new()
    {
        Id = server.Id,
        Name = server.Name,
        ServerType = server.ServerType.ToString(),
        Command = server.Command,
        Args = server.Args,
        SseUrl = server.SseUrl,
        Environment = server.Environment,
        IsEnabled = server.IsEnabled,
        Description = server.Description,
        Tools = server.Tools.Select(t => new McpToolDto
        {
            Id = t.Id,
            ServerId = t.ServerId,
            Name = t.Name,
            Description = t.Description,
            InputSchema = t.InputSchema,
            IsEnabled = t.IsEnabled
        }).ToList(),
        CreatedAt = server.CreatedAt,
        UpdatedAt = server.UpdatedAt
    };
}
