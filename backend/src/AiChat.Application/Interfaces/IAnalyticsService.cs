using AiChat.Application.DTOs;

namespace AiChat.Application.Interfaces;

/// <summary>
/// 统计分析服务接口
/// </summary>
public interface IAnalyticsService
{
    /// <summary>
    /// 获取系统概览统计
    /// </summary>
    Task<SystemOverviewDto> GetSystemOverviewAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取模型使用统计
    /// </summary>
    Task<IEnumerable<ModelUsageDto>> GetModelUsageStatsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取用户Token使用排行（前N名）
    /// </summary>
    Task<IEnumerable<UserTokenUsageDto>> GetTopUsersByTokenUsageAsync(int top = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取分组统计
    /// </summary>
    Task<IEnumerable<GroupStatsDto>> GetGroupStatsAsync(CancellationToken cancellationToken = default);
}
