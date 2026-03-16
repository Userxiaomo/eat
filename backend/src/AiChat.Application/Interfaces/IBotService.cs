using AiChat.Application.DTOs;

namespace AiChat.Application.Interfaces;

public interface IBotService
{
    Task<IEnumerable<BotDto>> GetVisibleBotsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<BotDto>> GetSystemBotsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<BotDto>> GetUserBotsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<BotDto?> GetBotByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BotDto> CreateBotAsync(Guid userId, CreateBotRequest request, CancellationToken cancellationToken = default);
    Task<BotDto> CreateSystemBotAsync(CreateBotRequest request, CancellationToken cancellationToken = default);
    Task<BotDto?> UpdateBotAsync(Guid id, Guid userId, UpdateBotRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteBotAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
}
