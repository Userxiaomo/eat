using AiChat.Application.DTOs;

namespace AiChat.Application.Interfaces;

public interface IUsageReportService
{
    /// <summary>
    /// 获取用户用量概览（包含月度配额）
    /// </summary>
    Task<UsageSummaryDto> GetUsageSummaryAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取每日用量趋势（默认最近30天）
    /// </summary>
    Task<IEnumerable<DailyUsageDto>> GetDailyUsageAsync(Guid userId, int days = 30, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取按模型分布的用量（默认本月）
    /// </summary>
    Task<IEnumerable<ModelUsageDto>> GetModelUsageAsync(Guid userId, CancellationToken cancellationToken = default);
}
