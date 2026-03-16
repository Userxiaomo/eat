namespace AiChat.Domain.Aggregates.UserAggregate;

public interface IUsageReportRepository
{
    Task<UsageReport?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取用户在指定日期的使用记录
    /// </summary>
    Task<UsageReport?> GetByUserDateModelAsync(
        Guid userId,
        DateOnly date,
        Guid modelId,
        Guid providerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取用户当月的总 Token 使用量
    /// </summary>
    Task<int> GetUserMonthlyTotalTokensAsync(
        Guid userId,
        int year,
        int month,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取用户的使用报告列表
    /// </summary>
    Task<IEnumerable<UsageReport>> GetUserReportsAsync(
        Guid userId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default);

    Task AddAsync(UsageReport report, CancellationToken cancellationToken = default);
    void Update(UsageReport report);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
