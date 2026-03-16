namespace AiChat.Application.DTOs;

/// <summary>
/// 分组 DTO
/// </summary>
public record GroupDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ModelType { get; init; } = "All";
    public string TokenLimitType { get; init; } = "Unlimited";
    public int? MonthlyTokenLimit { get; init; }
    public bool IsDefault { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public List<Guid> AllowedModelIds { get; init; } = new();
}

/// <summary>
/// 创建分组请求
/// </summary>
public record CreateGroupRequest
{
    public string Name { get; init; } = string.Empty;
    public int ModelType { get; init; } // 0 = All, 1 = Specific
    public int TokenLimitType { get; init; } // 0 = Unlimited, 1 = Limited
    public int? MonthlyTokenLimit { get; init; }
    public bool IsDefault { get; init; }
    public List<Guid>? AllowedModelIds { get; init; }
}

/// <summary>
/// 更新分组请求
/// </summary>
public record UpdateGroupRequest
{
    public string? Name { get; init; }
    public int? ModelType { get; init; }
    public int? TokenLimitType { get; init; }
    public int? MonthlyTokenLimit { get; init; }
    public bool? IsDefault { get; init; }
    public List<Guid>? AllowedModelIds { get; init; }
}

/// <summary>
/// 用户 DTO（扩展版，包含分组信息）
/// </summary>
public record UserWithGroupDto
{
    public Guid Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = "User";
    public Guid? GroupId { get; init; }
    public string? GroupName { get; init; }
    public int CurrentMonthTotalTokens { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// 用户 Token 使用统计 DTO
/// </summary>
public record UserUsageStatsDto
{
    public Guid UserId { get; init; }
    public int CurrentMonthTokens { get; init; }
    public int? MonthlyLimit { get; init; }
    public double UsagePercentage => MonthlyLimit.HasValue && MonthlyLimit > 0
        ? Math.Min(100, (double)CurrentMonthTokens / MonthlyLimit.Value * 100)
        : 0;
    public bool IsOverLimit => MonthlyLimit.HasValue && CurrentMonthTokens >= MonthlyLimit.Value;
}
