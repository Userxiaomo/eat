using AiChat.API.Filters;
using AiChat.Application.DTOs;
using AiChat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiChat.API.Controllers;

[Authorize]
[AdminOnly]
[ApiController]
[Route("api/admin/[controller]")]
public class ChannelsController : ControllerBase
{
    private readonly IChannelService _channelService;
    private readonly ILogger<ChannelsController> _logger;

    public ChannelsController(IChannelService channelService, ILogger<ChannelsController> logger)
    {
        _channelService = channelService ?? throw new ArgumentNullException(nameof(channelService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ChannelDto>>> GetChannels()
    {
        try
        {
            var channels = await _channelService.GetAllChannelsAsync();
            return Ok(channels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving channels");
            return StatusCode(500, new { message = "An error occurred while retrieving channels" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ChannelDto>> GetChannel(Guid id)
    {
        try
        {
            var channel = await _channelService.GetChannelByIdAsync(id);
            if (channel == null)
                return NotFound(new { message = "Channel not found" });

            return Ok(channel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving channel {ChannelId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the channel" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<ChannelDto>> CreateChannel([FromBody] CreateChannelRequest request)
    {
        try
        {
            var channel = await _channelService.CreateChannelAsync(request);
            return CreatedAtAction(nameof(GetChannel), new { id = channel.Id }, channel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating channel");
            return StatusCode(500, new { message = "An error occurred while creating the channel" });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ChannelDto>> UpdateChannel(Guid id, [FromBody] UpdateChannelRequest request)
    {
        try
        {
            var channel = await _channelService.UpdateChannelAsync(id, request);
            if (channel == null)
                return NotFound(new { message = "Channel not found" });

            return Ok(channel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating channel {ChannelId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the channel" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteChannel(Guid id)
    {
        try
        {
            var success = await _channelService.DeleteChannelAsync(id);
            if (!success)
                return NotFound(new { message = "Channel not found" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting channel {ChannelId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the channel" });
        }
    }

    /// <summary>
    /// 手动恢复渠道健康状态
    /// </summary>
    [HttpPost("{id}/recover-health")]
    public async Task<IActionResult> RecoverChannelHealth(Guid id)
    {
        try
        {
            var channel = await _channelService.GetChannelByIdAsync(id);
            if (channel == null)
                return NotFound(new { message = "Channel not found" });

            // 这里需要调用 ChannelRepository 直接操作
            // 暂时通过 IChannelLoadBalancer 来实现
            var loadBalancer = HttpContext.RequestServices.GetRequiredService<IChannelLoadBalancer>();
            await loadBalancer.MarkChannelSuccessAsync(id);

            return Ok(new { message = "Channel health recovered successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recovering channel health {ChannelId}", id);
            return StatusCode(500, new { message = "An error occurred while recovering channel health" });
        }
    }

}
