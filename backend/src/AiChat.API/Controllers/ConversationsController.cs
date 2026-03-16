using System.Security.Claims;
using AiChat.Application.DTOs;
using AiChat.Application.Interfaces;
using AiChat.Domain.Aggregates.ConversationAggregate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiChat.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ConversationsController : ControllerBase
{
    private readonly IConversationService _conversationService;
    private readonly ILogger<ConversationsController> _logger;

    public ConversationsController(IConversationService conversationService, ILogger<ConversationsController> logger)
    {
        _conversationService = conversationService ?? throw new ArgumentNullException(nameof(conversationService));
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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ConversationDto>>> GetConversations(
        [FromQuery] int? page = null,
        [FromQuery] int? pageSize = null)
    {
        try
        {
            var userId = GetUserId();

            // 如果提供了分页参数，使用分页查询
            if (page.HasValue && pageSize.HasValue)
            {
                var pagedResult = await _conversationService.GetUserConversationsPagedAsync(
                    userId, page.Value, pageSize.Value);
                return Ok(pagedResult);
            }

            // 否则返回全部（向后兼容）
            var conversations = await _conversationService.GetUserConversationsAsync(userId);
            return Ok(conversations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving conversations");
            return StatusCode(500, new { message = "An error occurred while retrieving conversations" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ConversationDto>> GetConversation(Guid id)
    {
        try
        {
            var userId = GetUserId();
            var conversation = await _conversationService.GetConversationByIdAsync(id, userId);

            if (conversation == null)
                return NotFound(new { message = "Conversation not found" });

            return Ok(conversation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving conversation {ConversationId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the conversation" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<ConversationDto>> CreateConversation([FromBody] CreateConversationRequest request)
    {
        try
        {
            var userId = GetUserId();
            var conversation = await _conversationService.CreateConversationAsync(userId, request);
            return CreatedAtAction(nameof(GetConversation), new { id = conversation.Id }, conversation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating conversation");
            return StatusCode(500, new { message = "An error occurred while creating the conversation" });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ConversationDto>> UpdateConversation(Guid id, [FromBody] UpdateConversationRequest request)
    {
        try
        {
            var userId = GetUserId();
            var conversation = await _conversationService.UpdateConversationAsync(id, userId, request);

            if (conversation == null)
                return NotFound(new { message = "Conversation not found" });

            return Ok(conversation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating conversation {ConversationId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the conversation" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteConversation(Guid id)
    {
        try
        {
            var userId = GetUserId();
            var success = await _conversationService.DeleteConversationAsync(id, userId);

            if (!success)
                return NotFound(new { message = "Conversation not found" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting conversation {ConversationId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the conversation" });
        }
    }

    [HttpGet("{id}/messages")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessages(Guid id)
    {
        try
        {
            var userId = GetUserId();
            var messages = await _conversationService.GetConversationMessagesAsync(id, userId);
            return Ok(messages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving messages for conversation {ConversationId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving messages" });
        }
    }

    [HttpPost("{id}/messages")]
    public async Task<ActionResult<SendMessageResponse>> SendMessage(Guid id, [FromBody] SendMessageRequest request)
    {
        try
        {
            var userId = GetUserId();
            var response = await _conversationService.SendMessageAsync(id, userId, request);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while sending message to conversation {ConversationId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to conversation {ConversationId}", id);
            return StatusCode(500, new { message = "An error occurred while sending the message" });
        }
    }

    /// <summary>
    /// 流式发送消息（SSE格式）
    /// </summary>
    [HttpPost("{id}/messages/stream")]
    public async Task SendMessageStream(Guid id, [FromBody] SendMessageRequest request)
    {
        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("Connection", "keep-alive");

        try
        {
            var userId = GetUserId();

            // 调用Service的同步方法获取完整响应
            var response = await _conversationService.SendMessageAsync(id, userId, request);

            // 模拟流式输出（将完整响应分块发送）
            var content = response.AssistantMessage?.Content ?? "";
            var chunkSize = 10; // 每次发送10个字符

            for (int i = 0; i < content.Length; i += chunkSize)
            {
                var chunk = content.Substring(i, Math.Min(chunkSize, content.Length - i));
                var sseData = $"data: {{\"delta\":\"{EscapeJson(chunk)}\",\"done\":false}}\n\n";
                await Response.WriteAsync(sseData);
                await Response.Body.FlushAsync();
                await Task.Delay(20); // 模拟延迟
            }

            // 发送完成信号
            await Response.WriteAsync($"data: {{\"delta\":\"\",\"done\":true}}\n\n");
            await Response.Body.FlushAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in stream for conversation {ConversationId}", id);
            await Response.WriteAsync($"data: {{\"error\":\"{EscapeJson(ex.Message)}\"}}\n\n");
            await Response.Body.FlushAsync();
        }
    }

    private static string EscapeJson(string text)
    {
        return text.Replace("\\", "\\\\")
                   .Replace("\"", "\\\"")
                   .Replace("\n", "\\n")
                   .Replace("\r", "\\r")
                   .Replace("\t", "\\t");
    }

    [HttpPost("{id}/star")]
    public async Task<IActionResult> ToggleStar(Guid id)
    {
        try
        {
            var userId = GetUserId();
            var isStar = await _conversationService.ToggleStarAsync(id, userId);
            return Ok(new { isStar });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling star for conversation {ConversationId}", id);
            return StatusCode(500, new { message = "An error occurred while toggling star" });
        }
    }

    [HttpPost("{id}/search")]
    public async Task<IActionResult> SetSearchEnabled(Guid id, [FromBody] bool enabled)
    {
        try
        {
            var userId = GetUserId();
            var success = await _conversationService.SetSearchEnabledAsync(id, userId, enabled);
            if (!success) return NotFound();
            return Ok(new { enabled });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting search enabled for conversation {ConversationId}", id);
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    [HttpPost("{id}/history")]
    public async Task<IActionResult> SetHistoryType(Guid id, [FromBody] SetHistoryTypeRequest request)
    {
        try
        {
            var userId = GetUserId();
            var success = await _conversationService.SetHistoryTypeAsync(id, userId, request.Type, request.Count);
            if (!success) return NotFound();
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting history type for conversation {ConversationId}", id);
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    [HttpPost("{id}/avatar")]
    public async Task<IActionResult> SetAvatar(Guid id, [FromBody] SetAvatarRequest request)
    {
        try
        {
            var userId = GetUserId();
            var success = await _conversationService.SetAvatarAsync(id, userId, request.Avatar, request.Type);
            if (!success) return NotFound();
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting avatar for conversation {ConversationId}", id);
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    [HttpDelete("{id}/messages/{messageId}")]
    public async Task<IActionResult> DeleteMessage(Guid id, Guid messageId)
    {
        try
        {
            var userId = GetUserId();
            var success = await _conversationService.SoftDeleteMessageAsync(id, messageId, userId);
            if (!success) return NotFound();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting message {MessageId} in conversation {ConversationId}", messageId, id);
            return StatusCode(500, new { message = "An error occurred" });
        }
    }
}
