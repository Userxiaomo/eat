using AiChat.Application.DTOs;
using AiChat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AiChat.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsageReportController : ControllerBase
{
    private readonly IUsageReportService _usageReportService;
    private readonly ILogger<UsageReportController> _logger;

    public UsageReportController(IUsageReportService usageReportService, ILogger<UsageReportController> logger)
    {
        _usageReportService = usageReportService ?? throw new ArgumentNullException(nameof(usageReportService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid or missing user token");
        }

        return userId;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<UsageSummaryDto>> GetSummary()
    {
        try
        {
            var userId = GetUserId();
            var summary = await _usageReportService.GetUsageSummaryAsync(userId);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving usage summary");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    [HttpGet("daily")]
    public async Task<ActionResult<IEnumerable<DailyUsageDto>>> GetDailyUsage([FromQuery] int days = 30)
    {
        try
        {
            var userId = GetUserId();
            var usage = await _usageReportService.GetDailyUsageAsync(userId, days);
            return Ok(usage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving daily usage");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    [HttpGet("models")]
    public async Task<ActionResult<IEnumerable<ModelUsageDto>>> GetModelUsage()
    {
        try
        {
            var userId = GetUserId();
            var usage = await _usageReportService.GetModelUsageAsync(userId);
            return Ok(usage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving model usage");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }
}
