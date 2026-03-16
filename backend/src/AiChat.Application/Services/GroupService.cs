using AiChat.Application.DTOs;
using AiChat.Application.Interfaces;
using AiChat.Domain.Aggregates.UserAggregate;

namespace AiChat.Application.Services;

public class GroupService : IGroupService
{
    private readonly IGroupRepository _groupRepository;
    private readonly IUserRepository _userRepository;

    public GroupService(IGroupRepository groupRepository, IUserRepository userRepository)
    {
        _groupRepository = groupRepository ?? throw new ArgumentNullException(nameof(groupRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task<IEnumerable<GroupDto>> GetAllGroupsAsync(CancellationToken cancellationToken = default)
    {
        var groups = await _groupRepository.GetAllAsync(cancellationToken);
        return groups.Select(MapToDto);
    }

    public async Task<GroupDto?> GetGroupByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var group = await _groupRepository.GetByIdAsync(id, cancellationToken);
        return group == null ? null : MapToDto(group);
    }

    public async Task<GroupDto> CreateGroupAsync(CreateGroupRequest request, CancellationToken cancellationToken = default)
    {
        var group = new Group(Guid.NewGuid(), request.Name, request.IsDefault);

        group.SetModelType((GroupModelType)request.ModelType);
        group.SetTokenLimit((TokenLimitType)request.TokenLimitType, request.MonthlyTokenLimit);

        // 如果是特定模型权限，添加允许的模型
        if (request.ModelType == (int)GroupModelType.Specific && request.AllowedModelIds != null)
        {
            foreach (var modelId in request.AllowedModelIds)
            {
                group.AddAllowedModel(modelId);
            }
        }

        // 如果设为默认分组，需要取消其他分组的默认状态
        if (request.IsDefault)
        {
            var currentDefault = await _groupRepository.GetDefaultGroupAsync(cancellationToken);
            if (currentDefault != null)
            {
                currentDefault.SetDefault(false);
                _groupRepository.Update(currentDefault);
            }
        }

        await _groupRepository.AddAsync(group, cancellationToken);
        await _groupRepository.SaveChangesAsync(cancellationToken);

        return MapToDto(group);
    }

    public async Task<GroupDto?> UpdateGroupAsync(Guid id, UpdateGroupRequest request, CancellationToken cancellationToken = default)
    {
        var group = await _groupRepository.GetByIdAsync(id, cancellationToken);
        if (group == null) return null;

        if (!string.IsNullOrEmpty(request.Name))
            group.UpdateName(request.Name);

        if (request.ModelType.HasValue)
            group.SetModelType((GroupModelType)request.ModelType.Value);

        if (request.TokenLimitType.HasValue)
            group.SetTokenLimit((TokenLimitType)request.TokenLimitType.Value, request.MonthlyTokenLimit);

        if (request.IsDefault.HasValue && request.IsDefault.Value && !group.IsDefault)
        {
            var currentDefault = await _groupRepository.GetDefaultGroupAsync(cancellationToken);
            if (currentDefault != null && currentDefault.Id != id)
            {
                currentDefault.SetDefault(false);
                _groupRepository.Update(currentDefault);
            }
            group.SetDefault(true);
        }

        if (request.AllowedModelIds != null)
        {
            group.ClearAllowedModels();
            foreach (var modelId in request.AllowedModelIds)
            {
                group.AddAllowedModel(modelId);
            }
        }

        _groupRepository.Update(group);
        await _groupRepository.SaveChangesAsync(cancellationToken);

        return MapToDto(group);
    }

    public async Task<bool> DeleteGroupAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var group = await _groupRepository.GetByIdAsync(id, cancellationToken);
        if (group == null) return false;

        _groupRepository.Delete(group);
        await _groupRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> IsUserOverTokenLimitAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var stats = await GetUserUsageStatsAsync(userId, cancellationToken);
        return stats?.IsOverLimit ?? false;
    }

    public async Task<UserUsageStatsDto?> GetUserUsageStatsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null) return null;

        int? monthlyLimit = null;

        if (user.GroupId.HasValue)
        {
            var group = await _groupRepository.GetByIdAsync(user.GroupId.Value, cancellationToken);
            if (group != null && group.TokenLimitType == TokenLimitType.Limited)
            {
                monthlyLimit = group.MonthlyTokenLimit;
            }
        }

        return new UserUsageStatsDto
        {
            UserId = userId,
            CurrentMonthTokens = user.CurrentMonthTotalTokens,
            MonthlyLimit = monthlyLimit
        };
    }

    public async Task<bool> CanUserAccessModelAsync(Guid userId, Guid modelId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null) return false;

        // 如果用户没有分组，默认可以访问所有模型
        if (!user.GroupId.HasValue) return true;

        var group = await _groupRepository.GetByIdAsync(user.GroupId.Value, cancellationToken);
        if (group == null) return true;

        // 如果分组允许所有模型
        if (group.ModelType == GroupModelType.All) return true;

        // 检查模型是否在允许列表中
        return group.AllowedModels.Any(m => m.ModelId == modelId);
    }

    private static GroupDto MapToDto(Group group)
    {
        return new GroupDto
        {
            Id = group.Id,
            Name = group.Name,
            ModelType = group.ModelType.ToString(),
            TokenLimitType = group.TokenLimitType.ToString(),
            MonthlyTokenLimit = group.MonthlyTokenLimit,
            IsDefault = group.IsDefault,
            CreatedAt = group.CreatedAt,
            UpdatedAt = group.UpdatedAt,
            AllowedModelIds = group.AllowedModels.Select(m => m.ModelId).ToList()
        };
    }
}
