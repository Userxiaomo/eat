namespace AiChat.Application.DTOs;

public record UsageSummaryDto
{
    public int CurrentMonthTotalTokens { get; init; }
    public int MonthlyLimit { get; init; }
    public double UsagePercentage => MonthlyLimit > 0 ? Math.Round((double)CurrentMonthTotalTokens / MonthlyLimit * 100, 2) : 0;
    public string PlanName { get; init; } = string.Empty;
}

public record DailyUsageDto
{
    public DateOnly Date { get; init; }
    public int InputTokens { get; init; }
    public int OutputTokens { get; init; }
    public int TotalTokens => InputTokens + OutputTokens;
}

public record ModelUsageDto
{
    public string ModelId { get; init; } = string.Empty;
    public string ModelName { get; init; } = string.Empty;

    // 统计字段
    public int ConversationCount { get; init; }
    public int MessageCount { get; init; }
    public long TotalInputTokens { get; init; }
    public long TotalOutputTokens { get; init; }
    public long TotalTokens { get; init; }
}
