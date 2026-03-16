using AiChat.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Runtime.CompilerServices;

namespace AiChat.Infrastructure.AI;

public class OpenAiService : IAiService
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService? _chatCompletionService;
    private readonly bool _isSimulated;
    private readonly ILogger<OpenAiService> _logger;

    public OpenAiService(Kernel kernel, ILogger<OpenAiService> logger)
    {
        _kernel = kernel;
        _logger = logger;

        // 尝试获取 Chat Completion Service，如果不存在则使用模拟模式
        try
        {
            _chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
            _isSimulated = false;
            _logger.LogInformation("OpenAI API 已启用（真实模式）");
        }
        catch
        {
            _isSimulated = true;
            _logger.LogWarning("OpenAI API Key 未配置，使用模拟响应模式");
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

        var chatHistory = BuildChatHistory(history, prompt, systemPrompt);

        var executionSettings = new PromptExecutionSettings
        {
            ExtensionData = new Dictionary<string, object>
            {
                { "temperature", temperature },
                { "max_tokens", maxTokens }
            }
        };

        var response = await _chatCompletionService!.GetChatMessageContentAsync(
            chatHistory,
            executionSettings,
            _kernel,
            cancellationToken);

        return response.Content ?? string.Empty;
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

        var chatHistory = BuildChatHistory(history, prompt, systemPrompt);

        var executionSettings = new PromptExecutionSettings
        {
            ExtensionData = new Dictionary<string, object>
            {
                { "temperature", temperature },
                { "max_tokens", maxTokens }
            }
        };

        await foreach (var chunk in _chatCompletionService!.GetStreamingChatMessageContentsAsync(
            chatHistory,
            executionSettings,
            _kernel,
            cancellationToken))
        {
            if (!string.IsNullOrEmpty(chunk.Content))
            {
                yield return chunk.Content;
            }
        }
    }

    /// <summary>
    /// OpenAI 不支持 reasoning_content，直接包装 StreamMessageAsync
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
    /// 从历史消息构建 ChatHistory
    /// </summary>
    private ChatHistory BuildChatHistory(IEnumerable<ChatMessage> history, string currentPrompt, string? systemPrompt)
    {
        var chatHistory = new ChatHistory();

        // 添加系统提示词
        if (!string.IsNullOrWhiteSpace(systemPrompt))
        {
            chatHistory.AddSystemMessage(systemPrompt);
        }

        // 添加历史消息
        foreach (var msg in history)
        {
            switch (msg.Role.ToLowerInvariant())
            {
                case "user":
                    chatHistory.AddUserMessage(msg.Content);
                    break;
                case "assistant":
                    chatHistory.AddAssistantMessage(msg.Content);
                    break;
                case "system":
                    chatHistory.AddSystemMessage(msg.Content);
                    break;
            }
        }

        // 添加当前用户消息
        chatHistory.AddUserMessage(currentPrompt);

        return chatHistory;
    }

    #region 模拟响应（用于无 API Key 时测试）

    private async Task<string> SimulateResponseAsync(string prompt)
    {
        await Task.Delay(500); // 模拟网络延迟
        return $"[OpenAI 模拟响应] 您的问题是：{prompt}\n\n这是一个模拟的 GPT 回复。要启用真实 AI 响应，请在 appsettings.json 中配置 OpenAI:ApiKey。";
    }

    private async IAsyncEnumerable<string> SimulateStreamingResponseAsync(
        string prompt,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var response = $"[OpenAI 模拟流式响应] 您好！这是针对您的问题 \"{prompt.Substring(0, Math.Min(20, prompt.Length))}...\" 的模拟回复。\n\n" +
                      "OpenAI API 当前处于模拟模式。要启用真实 AI 功能，请配置有效的 API Key。\n\n" +
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
