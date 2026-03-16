using AiChat.Application.DTOs;
using AiChat.Application.Interfaces;
using AiChat.Domain.Aggregates.ChannelAggregate;

namespace AiChat.Application.Services;

public class AiModelService : IAiModelService
{
    private readonly IAiModelRepository _modelRepository;
    private readonly IChannelRepository _channelRepository;

    public AiModelService(IAiModelRepository modelRepository, IChannelRepository channelRepository)
    {
        _modelRepository = modelRepository ?? throw new ArgumentNullException(nameof(modelRepository));
        _channelRepository = channelRepository ?? throw new ArgumentNullException(nameof(channelRepository));
    }

    public async Task<IEnumerable<AiModelDto>> GetAllModelsAsync(CancellationToken cancellationToken = default)
    {
        var channels = await _channelRepository.GetAllAsync(cancellationToken);
        var channelDict = channels.ToDictionary(c => c.Id, c => c.Name);

        var models = new List<AiModelDto>();
        foreach (var channel in channels)
        {
            foreach (var model in channel.Models)
            {
                models.Add(MapToDto(model, channel.Name));
            }
        }

        return models.OrderBy(m => m.SortOrder);
    }

    public async Task<IEnumerable<AiModelDto>> GetModelsByChannelAsync(Guid channelId, CancellationToken cancellationToken = default)
    {
        var channel = await _channelRepository.GetByIdAsync(channelId, cancellationToken);
        if (channel == null) return Enumerable.Empty<AiModelDto>();

        return channel.Models.Select(m => MapToDto(m, channel.Name)).OrderBy(m => m.SortOrder);
    }

    public async Task<AiModelDto?> GetModelByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var model = await _modelRepository.GetByIdAsync(id, cancellationToken);
        if (model == null) return null;

        var channel = await _channelRepository.GetByIdAsync(model.ChannelId, cancellationToken);
        return MapToDto(model, channel?.Name ?? "Unknown");
    }

    public async Task<AiModelDto> CreateModelAsync(CreateAiModelRequest request, CancellationToken cancellationToken = default)
    {
        var channel = await _channelRepository.GetByIdAsync(request.ChannelId, cancellationToken);
        if (channel == null)
            throw new InvalidOperationException("Channel not found.");

        var model = new AiModel(
            Guid.NewGuid(),
            request.ChannelId,
            request.Name,
            request.ModelId,
            request.ModelType,
            request.MaxTokens,
            request.InputPrice,
            request.OutputPrice,
            request.SortOrder
        );

        await _modelRepository.AddAsync(model, cancellationToken);
        await _modelRepository.SaveChangesAsync(cancellationToken);

        return MapToDto(model, channel.Name);
    }

    public async Task<AiModelDto?> UpdateModelAsync(Guid id, UpdateAiModelRequest request, CancellationToken cancellationToken = default)
    {
        var model = await _modelRepository.GetByIdAsync(id, cancellationToken);
        if (model == null) return null;

        model.Update(
            request.Name,
            request.ModelId,
            request.ModelType,
            request.MaxTokens,
            request.InputPrice,
            request.OutputPrice,
            request.SortOrder
        );
        model.SetEnabled(request.IsEnabled);

        await _modelRepository.SaveChangesAsync(cancellationToken);

        var channel = await _channelRepository.GetByIdAsync(model.ChannelId, cancellationToken);
        return MapToDto(model, channel?.Name ?? "Unknown");
    }

    public async Task<bool> DeleteModelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var model = await _modelRepository.GetByIdAsync(id, cancellationToken);
        if (model == null) return false;

        _modelRepository.Delete(model);
        await _modelRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static AiModelDto MapToDto(AiModel model, string channelName)
    {
        return new AiModelDto
        {
            Id = model.Id,
            ChannelId = model.ChannelId,
            ChannelName = channelName,
            Name = model.Name,
            ModelId = model.ModelId,
            ModelType = model.ModelType.ToString(),
            MaxTokens = model.MaxTokens,
            InputPrice = model.InputPrice,
            OutputPrice = model.OutputPrice,
            IsEnabled = model.IsEnabled,
            SortOrder = model.SortOrder,
            CreatedAt = model.CreatedAt
        };
    }
}
