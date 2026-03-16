using AiChat.Application.DTOs;

namespace AiChat.Application.Interfaces;

public interface IChannelService
{
    Task<IEnumerable<ChannelDto>> GetAllChannelsAsync(CancellationToken cancellationToken = default);
    Task<ChannelDto?> GetChannelByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ChannelDto> CreateChannelAsync(CreateChannelRequest request, CancellationToken cancellationToken = default);
    Task<ChannelDto?> UpdateChannelAsync(Guid id, UpdateChannelRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteChannelAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 从渠道的多个 API Key 中随机选择一个（支持换行分隔）
    /// </summary>
    Task<string?> GetRandomApiKeyAsync(Guid channelId, CancellationToken cancellationToken = default);
}


public interface IAiModelService
{
    Task<IEnumerable<AiModelDto>> GetAllModelsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<AiModelDto>> GetModelsByChannelAsync(Guid channelId, CancellationToken cancellationToken = default);
    Task<AiModelDto?> GetModelByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AiModelDto> CreateModelAsync(CreateAiModelRequest request, CancellationToken cancellationToken = default);
    Task<AiModelDto?> UpdateModelAsync(Guid id, UpdateAiModelRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteModelAsync(Guid id, CancellationToken cancellationToken = default);
}
