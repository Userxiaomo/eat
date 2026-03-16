using System.Security.Claims;
using AiChat.Application.DTOs;
using AiChat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiChat.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BotsController : ControllerBase
{
    private readonly IBotService _botService;
    private readonly ILogger<BotsController> _logger;

    public BotsController(IBotService botService, ILogger<BotsController> logger)
    {
        _botService = botService;
        _logger = logger;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    /// <summary>
    /// 获取用户可见的所有 Bot
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BotDto>>> GetBots(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var bots = await _botService.GetVisibleBotsAsync(userId, cancellationToken);
        return Ok(bots);
    }

    /// <summary>
    /// 获取系统预设 Bot
    /// </summary>
    [HttpGet("system")]
    public async Task<ActionResult<IEnumerable<BotDto>>> GetSystemBots(CancellationToken cancellationToken)
    {
        var bots = await _botService.GetSystemBotsAsync(cancellationToken);
        return Ok(bots);
    }

    /// <summary>
    /// 获取用户创建的 Bot
    /// </summary>
    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<BotDto>>> GetMyBots(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var bots = await _botService.GetUserBotsAsync(userId, cancellationToken);
        return Ok(bots);
    }

    /// <summary>
    /// 获取单个 Bot
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BotDto>> GetBot(Guid id, CancellationToken cancellationToken)
    {
        var bot = await _botService.GetBotByIdAsync(id, cancellationToken);
        if (bot == null)
            return NotFound();

        return Ok(bot);
    }

    /// <summary>
    /// 创建用户 Bot
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<BotDto>> CreateBot(CreateBotRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            var bot = await _botService.CreateBotAsync(userId, request, cancellationToken);
            return CreatedAtAction(nameof(GetBot), new { id = bot.Id }, bot);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 创建系统 Bot（仅管理员）
    /// </summary>
    [HttpPost("system")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<BotDto>> CreateSystemBot(CreateBotRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var bot = await _botService.CreateSystemBotAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetBot), new { id = bot.Id }, bot);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 更新 Bot
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BotDto>> UpdateBot(Guid id, UpdateBotRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var bot = await _botService.UpdateBotAsync(id, userId, request, cancellationToken);
        if (bot == null)
            return NotFound();

        return Ok(bot);
    }

    /// <summary>
    /// 删除 Bot
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteBot(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var deleted = await _botService.DeleteBotAsync(id, userId, cancellationToken);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
