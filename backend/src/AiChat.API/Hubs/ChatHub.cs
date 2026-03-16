using System.Security.Claims;
using AiChat.Application.DTOs;
using AiChat.Application.Interfaces;
using AiChat.Infrastructure.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AiChat.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IConversationService _conversationService;
    private readonly AiServiceFactory _aiServiceFactory;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(
        IConversationService conversationService,
        AiServiceFactory aiServiceFactory,
        ILogger<ChatHub> logger)
    {
        _conversationService = conversationService;
        _aiServiceFactory = aiServiceFactory;
        _logger = logger;
    }

    private Guid GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new HubException("Invalid or missing user token");
        }

        return userId;
    }

    public async Task SendMessage(Guid conversationId, string content)
    {
        try
        {
            var userId = GetUserId();

            // Validate conversation access
            var conversation = await _conversationService.GetConversationByIdAsync(conversationId, userId);
            if (conversation == null)
            {
                throw new HubException("Conversation not found or access denied");
            }

            // 获取历史消息（用于 AI 上下文）
            var existingMessages = await _conversationService.GetConversationMessagesAsync(conversationId, userId);
            var history = existingMessages.Select(m => new ChatMessage(m.Role, m.Content)).ToList();

            // Save user message
            var userMessage = await _conversationService.AddUserMessageAsync(conversationId, userId, content);

            // Send user message confirmation
            await Clients.Caller.SendAsync("UserMessageSaved", userMessage);

            // 根据模型ID获取对应的 AI 服务
            var aiService = _aiServiceFactory.GetService(conversation.ModelId);

            // 使用带思维链的流式调用
            var result = await aiService.StreamMessageWithReasoningAsync(
                content,
                history,
                conversation.ModelId,
                // 内容回调
                async (chunk) => await Clients.Caller.SendAsync("MessageChunk", chunk),
                // 思维链回调
                async (reasoning) => await Clients.Caller.SendAsync("ReasoningChunk", reasoning),
                (float)conversation.Temperature,
                conversation.MaxTokens,
                conversation.SystemPrompt);

            // Save assistant message with reasoning and token usage
            var assistantMessage = await _conversationService.AddAssistantMessageAsync(
                conversationId,
                userId,
                result.Content,
                result.ReasoningContent,
                result.InputTokens,
                result.OutputTokens);

            // Send completion notification
            await Clients.Caller.SendAsync("MessageComplete", assistantMessage);
        }
        catch (HubException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SendMessage hub method");
            throw new HubException("An error occurred while processing your message");
        }
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        _logger.LogInformation("User {UserId} connected to ChatHub", userId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        _logger.LogInformation("User {UserId} disconnected from ChatHub", userId);
        await base.OnDisconnectedAsync(exception);
    }
}
