namespace AiChat.Application.DTOs;

// ========== 统计分析 DTOs ==========

/// <summary>
/// 系统概览统计
/// </summary>
public record SystemOverviewDto
{
    public int TotalUsers { get; init; }
    public int ActiveUsers { get; init; }  // 本月有使用的用户
    public int TotalConversations { get; init; }
    public int TotalMessages { get; init; }
    public long TotalTokensUsed { get; init; }
}

/// <summary>
/// 用户 Token 使用排行
/// </summary>
public record UserTokenUsageDto
{
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public int CurrentMonthTokens { get; init; }
    public DateTime UsageUpdatedAt { get; init; }
}

/// <summary>
/// 分组统计
/// </summary>
public record GroupStatsDto
{
    public Guid? GroupId { get; init; }
    public string? GroupName { get; init; }
    public int UserCount { get; init; }
    public int ConversationCount { get; init; }
}
