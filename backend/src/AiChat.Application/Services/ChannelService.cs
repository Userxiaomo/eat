using AiChat.Application.DTOs;
using AiChat.Application.Interfaces;
using AiChat.Domain.Aggregates.ChannelAggregate;

namespace AiChat.Application.Services;

public class ChannelService : IChannelService
{
    private readonly IChannelRepository _channelRepository;
    private readonly IEncryptionService _encryptionService;

    public ChannelService(IChannelRepository channelRepository, IEncryptionService encryptionService)
    {
        _channelRepository = channelRepository ?? throw new ArgumentNullException(nameof(channelRepository));
        _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
    }

    public async Task<IEnumerable<ChannelDto>> GetAllChannelsAsync(CancellationToken cancellationToken = default)
    {
        var channels = await _channelRepository.GetAllAsync(cancellationToken);

        return channels.Select(c => new ChannelDto
        {
            Id = c.Id,
            Name = c.Name,
            Provider = c.Provider.ToString(),
            BaseUrl = c.BaseUrl,
            IsEnabled = c.IsEnabled,
            CreatedAt = c.CreatedAt,
            ModelCount = c.Models.Count,
            Priority = c.Priority,
            Weight = c.Weight,
            MaxRetries = c.MaxRetries,
            IsHealthy = c.IsHealthy,
            LastFailedAt = c.LastFailedAt
        });
    }

    public async Task<ChannelDto?> GetChannelByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var channel = await _channelRepository.GetByIdAsync(id, cancellationToken);
        if (channel == null) return null;

        return new ChannelDto
        {
            Id = channel.Id,
            Name = channel.Name,
            Provider = channel.Provider.ToString(),
            BaseUrl = channel.BaseUrl,
            IsEnabled = channel.IsEnabled,
            CreatedAt = channel.CreatedAt,
            ModelCount = channel.Models.Count,
            Priority = channel.Priority,
            Weight = channel.Weight,
            MaxRetries = channel.MaxRetries,
            IsHealthy = channel.IsHealthy,
            LastFailedAt = channel.LastFailedAt
        };
    }

    public async Task<ChannelDto> CreateChannelAsync(CreateChannelRequest request, CancellationToken cancellationToken = default)
    {
        // 加密 API Key
        var encryptedApiKey = _encryptionService.Encrypt(request.ApiKey);

        var channel = new Channel(
            Guid.NewGuid(),
            request.Name,
            request.Provider,
            encryptedApiKey,
            request.BaseUrl
        );

        await _channelRepository.AddAsync(channel, cancellationToken);
        await _channelRepository.SaveChangesAsync(cancellationToken);

        return new ChannelDto
        {
            Id = channel.Id,
            Name = channel.Name,
            Provider = channel.Provider.ToString(),
            BaseUrl = channel.BaseUrl,
            IsEnabled = channel.IsEnabled,
            CreatedAt = channel.CreatedAt,
            ModelCount = 0,
            Priority = channel.Priority,
            Weight = channel.Weight,
            MaxRetries = channel.MaxRetries,
            IsHealthy = channel.IsHealthy,
            LastFailedAt = channel.LastFailedAt
        };
    }

    public async Task<ChannelDto?> UpdateChannelAsync(Guid id, UpdateChannelRequest request, CancellationToken cancellationToken = default)
    {
        var channel = await _channelRepository.GetByIdAsync(id, cancellationToken);
        if (channel == null) return null;

        // 如果提供了新的 API Key，加密后更新
        string? encryptedApiKey = null;
        if (!string.IsNullOrEmpty(request.ApiKey))
        {
            encryptedApiKey = _encryptionService.Encrypt(request.ApiKey);
        }

        channel.Update(request.Name, request.Provider, encryptedApiKey, request.BaseUrl);
        channel.SetEnabled(request.IsEnabled);

        // 更新负载均衡相关设置
        if (request.Priority.HasValue)
            channel.SetPriority(request.Priority.Value);
        if (request.Weight.HasValue)
            channel.SetWeight(request.Weight.Value);
        if (request.MaxRetries.HasValue)
            channel.SetMaxRetries(request.MaxRetries.Value);

        await _channelRepository.SaveChangesAsync(cancellationToken);

        return new ChannelDto
        {
            Id = channel.Id,
            Name = channel.Name,
            Provider = channel.Provider.ToString(),
            BaseUrl = channel.BaseUrl,
            IsEnabled = channel.IsEnabled,
            CreatedAt = channel.CreatedAt,
            ModelCount = channel.Models.Count,
            Priority = channel.Priority,
            Weight = channel.Weight,
            MaxRetries = channel.MaxRetries,
            IsHealthy = channel.IsHealthy,
            LastFailedAt = channel.LastFailedAt
        };
    }

    public async Task<bool> DeleteChannelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var channel = await _channelRepository.GetByIdAsync(id, cancellationToken);
        if (channel == null) return false;

        _channelRepository.Delete(channel);
        await _channelRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <summary>
    /// 从渠道的多个 API Key 中随机选择一个
    /// 支持换行符分隔多个 Key
    /// </summary>
    public async Task<string?> GetRandomApiKeyAsync(Guid channelId, CancellationToken cancellationToken = default)
    {
        var channel = await _channelRepository.GetByIdAsync(channelId, cancellationToken);
        if (channel == null)
            return null;

        // 解密 API Key
        var decryptedKey = _encryptionService.Decrypt(channel.ApiKey);

        // 按换行符分割
        var keys = decryptedKey.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(k => k.Trim())
            .Where(k => !string.IsNullOrEmpty(k))
            .ToList();

        if (!keys.Any())
            return null;

        // 如果只有一个 Key，直接返回
        if (keys.Count == 1)
            return keys[0];

        // 随机选择一个
        var randomIndex = Random.Shared.Next(keys.Count);
        return keys[randomIndex];
    }
}

