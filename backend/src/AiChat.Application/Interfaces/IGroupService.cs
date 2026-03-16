using AiChat.Application.DTOs;

namespace AiChat.Application.Interfaces;

public interface IGroupService
{
    Task<IEnumerable<GroupDto>> GetAllGroupsAsync(CancellationToken cancellationToken = default);
    Task<GroupDto?> GetGroupByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<GroupDto> CreateGroupAsync(CreateGroupRequest request, CancellationToken cancellationToken = default);
    Task<GroupDto?> UpdateGroupAsync(Guid id, UpdateGroupRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteGroupAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查用户是否超过 Token 限额
    /// </summary>
    Task<bool> IsUserOverTokenLimitAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取用户的使用统计
    /// </summary>
    Task<UserUsageStatsDto?> GetUserUsageStatsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查用户是否可以使用指定模型
    /// </summary>
    Task<bool> CanUserAccessModelAsync(Guid userId, Guid modelId, CancellationToken cancellationToken = default);
}
