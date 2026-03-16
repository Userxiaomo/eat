using AiChat.Application.Interfaces;
using Anthropic.SDK;
using Anthropic.SDK.Constants;
using Anthropic.SDK.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace AiChat.Infrastructure.AI;

public class ClaudeService : IAiService
{
    private readonly AnthropicClient? _client;
    private readonly bool _isSimulated;
    private readonly ILogger<ClaudeService> _logger;

    public ClaudeService(IConfiguration configuration, ILogger<ClaudeService> logger)
    {
        _logger = logger;
        var apiKey = configuration["Claude:ApiKey"];

        // 如果没有 API Key 或为占位符，使用模拟模式
        if (string.IsNullOrEmpty(apiKey) || apiKey == "your-claude-api-key-here")
        {
            _isSimulated = true;
            _logger.LogWarning("Claude API Key 未配置，使用模拟响应模式");
        }
        else
        {
            _isSimulated = false;
            _client = new AnthropicClient(apiKey);
            _logger.LogInformation("Claude API 已启用（真实模式）");
        }
    }

    public async Task<string> SendMessageAsync(
        string prompt,
        IEnumerable<ChatMessage> history,
        string modelId,
        float temperature = 0.7f,
        int maxTokens = 2000,
        string? systemPrompt = null,
        CancellationToken cancellationToken = default)
    {
        if (_isSimulated)
        {
            return await SimulateResponseAsync(prompt);
        }

        var messages = BuildClaudeMessages(history, prompt);

        var parameters = new MessageParameters
        {
            Messages = messages,
            Model = modelId,
            MaxTokens = maxTokens,
            Temperature = (decimal)temperature,
            Stream = false,
            System = string.IsNullOrWhiteSpace(systemPrompt) ? null : new List<SystemMessage> { new(systemPrompt) }
        };

        var response = await _client!.Messages.GetClaudeMessageAsync(parameters, cancellationToken);
        return response.Message?.ToString() ?? string.Empty;
    }

    public async IAsyncEnumerable<string> StreamMessageAsync(
        string prompt,
        IEnumerable<ChatMessage> history,
        string modelId,
        float temperature = 0.7f,
        int maxTokens = 2000,
        string? systemPrompt = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (_isSimulated)
        {
            await foreach (var chunk in SimulateStreamingResponseAsync(prompt, cancellationToken))
            {
                yield return chunk;
            }
            yield break;
        }

        var messages = BuildClaudeMessages(history, prompt);

        var parameters = new MessageParameters
        {
            Messages = messages,
            Model = modelId,
            MaxTokens = maxTokens,
            Temperature = (decimal)temperature,
            Stream = true,
            System = string.IsNullOrWhiteSpace(systemPrompt) ? null : new List<SystemMessage> { new(systemPrompt) }
        };

        await foreach (var chunk in _client!.Messages.StreamClaudeMessageAsync(parameters, cancellationToken))
        {
            // 提取文本内容（SDK 5.8.0+ 直接返回内容）
            var text = chunk?.ToString();
            if (!string.IsNullOrEmpty(text))
            {
                yield return text;
            }
        }
    }

    /// <summary>
    /// Claude 不支持 reasoning_content，直接包装 StreamMessageAsync
    /// </summary>
    public async Task<StreamResult> StreamMessageWithReasoningAsync(
        string prompt,
        IEnumerable<ChatMessage> history,
        string modelId,
        Func<string, Task> onContentChunk,
        Func<string, Task>? onReasoningChunk = null,
        float temperature = 0.7f,
        int maxTokens = 2000,
        string? systemPrompt = null,
        CancellationToken cancellationToken = default)
    {
        var result = new StreamResult();
        var contentBuilder = new System.Text.StringBuilder();

        await foreach (var chunk in StreamMessageAsync(prompt, history, modelId, temperature, maxTokens, systemPrompt, cancellationToken))
        {
            contentBuilder.Append(chunk);
            await onContentChunk(chunk);
        }

        result.Content = contentBuilder.ToString();
        return result;
    }

    /// <summary>
    /// 从历史消息构建 Claude 消息列表
    /// </summary>
    private List<Message> BuildClaudeMessages(IEnumerable<ChatMessage> history, string currentPrompt)
    {
        var messages = new List<Message>();

        // 添加历史消息
        foreach (var msg in history)
        {
            var role = msg.Role.ToLowerInvariant() switch
            {
                "user" => RoleType.User,
                "assistant" => RoleType.Assistant,
                _ => RoleType.User // 默认当作用户消息
            };

            messages.Add(new Message(role, msg.Content));
        }

        // 添加当前用户消息
        messages.Add(new Message(RoleType.User, currentPrompt));

        return messages;
    }

    #region 模拟响应（用于无 API Key 时测试）

    private async Task<string> SimulateResponseAsync(string prompt)
    {
        await Task.Delay(500); // 模拟网络延迟
        return $"[Claude 模拟响应] 您的问题是：{prompt}\n\n这是一个模拟的 Claude 回复。要启用真实 AI 响应，请在 appsettings.json 中配置 Claude:ApiKey。";
    }

    private async IAsyncEnumerable<string> SimulateStreamingResponseAsync(
        string prompt,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var response = $"[Claude 模拟流式响应] 您好！这是针对您的问题 \"{prompt.Substring(0, Math.Min(20, prompt.Length))}...\" 的模拟回复。\n\n" +
                      "Claude API 当前处于模拟模式。要启用真实 AI 功能，请配置有效的 API Key。\n\n" +
                      "模拟流式输出完成。";

        var words = response.Split(' ');
        foreach (var word in words)
        {
            if (cancellationToken.IsCancellationRequested)
                yield break;

            yield return word + " ";
            await Task.Delay(50, cancellationToken); // 模拟流式输出延迟
        }
    }

    #endregion
}
