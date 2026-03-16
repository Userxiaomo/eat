using AiChat.Application.DTOs;

namespace AiChat.Application.Interfaces;

/// <summary>
/// AI 聊天消息（用于传递历史上下文）
/// </summary>
public record ChatMessage(string Role, string Content);

/// <summary>
/// 流式响应结果（包含思维链和 Token 统计）
/// </summary>
public class StreamResult
{
    public string Content { get; set; } = string.Empty;
    public string? ReasoningContent { get; set; }
    public int? InputTokens { get; set; }
    public int? OutputTokens { get; set; }
}

public interface IAiService
{
    /// <summary>
    /// 发送消息并获取完整响应
    /// </summary>
    Task<string> SendMessageAsync(
        string prompt,
        IEnumerable<ChatMessage> history,
        string modelId,
        float temperature = 0.7f,
        int maxTokens = 2000,
        string? systemPrompt = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送消息并获取流式响应
    /// </summary>
    IAsyncEnumerable<string> StreamMessageAsync(
        string prompt,
        IEnumerable<ChatMessage> history,
        string modelId,
        float temperature = 0.7f,
        int maxTokens = 2000,
        string? systemPrompt = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送消息并获取流式响应（带思维链和回调）
    /// </summary>
    Task<StreamResult> StreamMessageWithReasoningAsync(
        string prompt,
        IEnumerable<ChatMessage> history,
        string modelId,
        Func<string, Task> onContentChunk,
        Func<string, Task>? onReasoningChunk = null,
        float temperature = 0.7f,
        int maxTokens = 2000,
        string? systemPrompt = null,
        CancellationToken cancellationToken = default);
}
