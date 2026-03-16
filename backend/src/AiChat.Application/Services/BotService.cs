using AiChat.Application.DTOs;
using AiChat.Application.Interfaces;
using AiChat.Domain.Aggregates.BotAggregate;

namespace AiChat.Application.Services;

public class BotService : IBotService
{
    private readonly IBotRepository _botRepository;

    public BotService(IBotRepository botRepository)
    {
        _botRepository = botRepository ?? throw new ArgumentNullException(nameof(botRepository));
    }

    public async Task<IEnumerable<BotDto>> GetVisibleBotsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var bots = await _botRepository.GetVisibleBotsAsync(userId, cancellationToken);
        return bots.Select(MapToDto);
    }

    public async Task<IEnumerable<BotDto>> GetSystemBotsAsync(CancellationToken cancellationToken = default)
    {
        var bots = await _botRepository.GetSystemBotsAsync(cancellationToken);
        return bots.Select(MapToDto);
    }

    public async Task<IEnumerable<BotDto>> GetUserBotsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var bots = await _botRepository.GetUserBotsAsync(userId, cancellationToken);
        return bots.Select(MapToDto);
    }

    public async Task<BotDto?> GetBotByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var bot = await _botRepository.GetByIdAsync(id, cancellationToken);
        return bot == null ? null : MapToDto(bot);
    }

    public async Task<BotDto> CreateBotAsync(Guid userId, CreateBotRequest request, CancellationToken cancellationToken = default)
    {
        var bot = new Bot(Guid.NewGuid(), request.Name, request.SystemPrompt, isSystem: false, createdByUserId: userId);

        bot.UpdateInfo(request.Name, request.Description, request.SystemPrompt);
        bot.SetAvatar(request.AvatarUrl);
        bot.SetModel(request.ModelId);
        bot.SetPublic(request.IsPublic);
        bot.SetWebSearch(request.EnableWebSearch);

        await _botRepository.AddAsync(bot, cancellationToken);
        await _botRepository.SaveChangesAsync(cancellationToken);

        return MapToDto(bot);
    }

    public async Task<BotDto> CreateSystemBotAsync(CreateBotRequest request, CancellationToken cancellationToken = default)
    {
        var bot = new Bot(Guid.NewGuid(), request.Name, request.SystemPrompt, isSystem: true, createdByUserId: null);

        bot.UpdateInfo(request.Name, request.Description, request.SystemPrompt);
        bot.SetAvatar(request.AvatarUrl);
        bot.SetModel(request.ModelId);
        bot.SetPublic(true); // 系统 Bot 默认公开
        bot.SetWebSearch(request.EnableWebSearch);

        await _botRepository.AddAsync(bot, cancellationToken);
        await _botRepository.SaveChangesAsync(cancellationToken);

        return MapToDto(bot);
    }

    public async Task<BotDto?> UpdateBotAsync(Guid id, Guid userId, UpdateBotRequest request, CancellationToken cancellationToken = default)
    {
        var bot = await _botRepository.GetByIdAsync(id, cancellationToken);
        if (bot == null) return null;

        // 只有创建者或管理员可以编辑（系统 Bot 只有管理员可以编辑，这里简化处理）
        if (!bot.IsSystem && bot.CreatedByUserId != userId)
            return null;

        if (request.Name != null || request.Description != null || request.SystemPrompt != null)
        {
            bot.UpdateInfo(
                request.Name ?? bot.Name,
                request.Description ?? bot.Description,
                request.SystemPrompt ?? bot.SystemPrompt
            );
        }

        if (request.AvatarUrl != null)
            bot.SetAvatar(request.AvatarUrl);

        if (request.ModelId.HasValue)
            bot.SetModel(request.ModelId);

        if (request.IsPublic.HasValue)
            bot.SetPublic(request.IsPublic.Value);

        if (request.EnableWebSearch.HasValue)
            bot.SetWebSearch(request.EnableWebSearch.Value);

        if (request.SortOrder.HasValue)
            bot.SetSortOrder(request.SortOrder.Value);

        _botRepository.Update(bot);
        await _botRepository.SaveChangesAsync(cancellationToken);

        return MapToDto(bot);
    }

    public async Task<bool> DeleteBotAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var bot = await _botRepository.GetByIdAsync(id, cancellationToken);
        if (bot == null) return false;

        // 系统 Bot 不能删除，只有创建者可以删除自己的 Bot
        if (bot.IsSystem || bot.CreatedByUserId != userId)
            return false;

        _botRepository.Delete(bot);
        await _botRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static BotDto MapToDto(Bot bot) => new()
    {
        Id = bot.Id,
        Name = bot.Name,
        Description = bot.Description,
        SystemPrompt = bot.SystemPrompt,
        AvatarUrl = bot.AvatarUrl,
        ModelId = bot.ModelId,
        IsSystem = bot.IsSystem,
        CreatedByUserId = bot.CreatedByUserId,
        IsPublic = bot.IsPublic,
        EnableWebSearch = bot.EnableWebSearch,
        SortOrder = bot.SortOrder,
        CreatedAt = bot.CreatedAt,
        UpdatedAt = bot.UpdatedAt
    };
}
