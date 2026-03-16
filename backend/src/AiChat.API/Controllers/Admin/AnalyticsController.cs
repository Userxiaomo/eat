using AiChat.Application.DTOs;
using AiChat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiChat.API.Controllers.Admin;

/// <summary>
/// 统计分析管理接口（仅管理员）
/// </summary>
[ApiController]
[Route("api/admin/analytics")]
[Authorize(Roles = "Admin")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
    }

    /// <summary>
    /// 获取系统概览统计
    /// </summary>
    [HttpGet("overview")]
    public async Task<ActionResult<SystemOverviewDto>> GetSystemOverview(CancellationToken cancellationToken)
    {
        var overview = await _analyticsService.GetSystemOverviewAsync(cancellationToken);
        return Ok(overview);
    }

    /// <summary>
    /// 获取模型使用统计
    /// </summary>
    [HttpGet("models")]
    public async Task<ActionResult<IEnumerable<ModelUsageDto>>> GetModelUsageStats(CancellationToken cancellationToken)
    {
        var stats = await _analyticsService.GetModelUsageStatsAsync(cancellationToken);
        return Ok(stats);
    }

    /// <summary>
    /// 获取用户Token使用排行
    /// </summary>
    /// <param name="top">返回前N名，默认10</param>
    [HttpGet("users/top")]
    public async Task<ActionResult<IEnumerable<UserTokenUsageDto>>> GetTopUsers(
        [FromQuery] int top = 10,
        CancellationToken cancellationToken = default)
    {
        if (top < 1 || top > 100)
            return BadRequest("top参数必须在1-100之间");

        var topUsers = await _analyticsService.GetTopUsersByTokenUsageAsync(top, cancellationToken);
        return Ok(topUsers);
    }

    /// <summary>
    /// 获取分组统计
    /// </summary>
    [HttpGet("groups")]
    public async Task<ActionResult<IEnumerable<GroupStatsDto>>> GetGroupStats(CancellationToken cancellationToken)
    {
        var stats = await _analyticsService.GetGroupStatsAsync(cancellationToken);
        return Ok(stats);
    }
}
