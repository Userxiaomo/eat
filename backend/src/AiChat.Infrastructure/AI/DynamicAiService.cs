using AiChat.Application.Interfaces;
using AiChat.Domain.Aggregates.ChannelAggregate;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Runtime.CompilerServices;

namespace AiChat.Infrastructure.AI;

// 使用别名解决类型冲突
using AppChatMessage = AiChat.Application.Interfaces.ChatMessage;

/// <summary>
/// 动态 AI 服务 - 从数据库读取渠道配置
/// </summary>
public class DynamicAiService : IAiService
{
    private readonly IAiModelRepository _modelRepository;
    private readonly IChannelRepository _channelRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<DynamicAiService> _logger;

    public DynamicAiService(
        IAiModelRepository modelRepository,
        IChannelRepository channelRepository,
        IEncryptionService encryptionService,
        ILogger<DynamicAiService> logger)
    {
        _modelRepository = modelRepository;
        _channelRepository = channelRepository;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public async Task<string> SendMessageAsync(
        string prompt,
        IEnumerable<AppChatMessage> history,
        string modelId,
        float temperature = 0.7f,
        int maxTokens = 2000,
        string? systemPrompt = null,
        CancellationToken cancellationToken = default)
    {
        var result = new System.Text.StringBuilder();
        await foreach (var chunk in StreamMessageAsync(prompt, history, modelId, temperature, maxTokens, systemPrompt, cancellationToken))
        {
            result.Append(chunk);
        }
        return result.ToString();
    }

    public async IAsyncEnumerable<string> StreamMessageAsync(
        string prompt,
        IEnumerable<AppChatMessage> history,
        string modelId,
        float temperature = 0.7f,
        int maxTokens = 2000,
        string? systemPrompt = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // 从数据库获取模型和渠道信息
        var channels = await _channelRepository.GetAllAsync(cancellationToken);
        AiModel? aiModel = null;
        Channel? channel = null;

        foreach (var c in channels)
        {
            var model = c.Models.FirstOrDefault(m => m.ModelId == modelId && m.IsEnabled);
            if (model != null)
            {
                aiModel = model;
                channel = c;
                break;
            }
        }

        if (channel == null || aiModel == null)
        {
            _logger.LogWarning("未找到模型 {ModelId} 的配置，使用模拟响应", modelId);
            yield return $"[模拟响应] 未找到模型 {modelId} 的配置。请在管理后台添加对应的渠道和模型。";
            yield break;
        }

        // 解密 API Key
        var apiKey = _encryptionService.Decrypt(channel.ApiKey);

        if (string.IsNullOrEmpty(apiKey))
        {
            yield return "[错误] API Key 未配置";
            yield break;
        }

        _logger.LogInformation("使用渠道 {ChannelName} 调用模型 {ModelId}，API地址: {BaseUrl}", channel.Name, modelId, channel.BaseUrl);

        // 根据提供商类型调用不同的 API
        var provider = channel.Provider;

        if (provider == Provider.OpenAI || provider == Provider.Azure || provider == Provider.DeepSeek || provider == Provider.Qwen || provider == Provider.Custom)
        {
            await foreach (var chunk in CallOpenAiCompatibleApi(apiKey, channel.BaseUrl, modelId, prompt, history, temperature, maxTokens, systemPrompt, cancellationToken))
            {
                yield return chunk;
            }
        }
        else if (provider == Provider.Claude)
        {
            await foreach (var chunk in CallClaudeApi(apiKey, channel.BaseUrl, modelId, prompt, history, temperature, maxTokens, systemPrompt, cancellationToken))
            {
                yield return chunk;
            }
        }
    }

    private async IAsyncEnumerable<string> CallOpenAiCompatibleApi(
        string apiKey,
        string baseUrl,
        string modelId,
        string prompt,
        IEnumerable<AppChatMessage> history,
        float temperature,
        int maxTokens,
        string? systemPrompt,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        OpenAIClientOptions? options = null;
        if (!string.IsNullOrEmpty(baseUrl))
        {
            options = new OpenAIClientOptions { Endpoint = new Uri(baseUrl) };
        }

        var client = options != null
            ? new OpenAIClient(new ApiKeyCredential(apiKey), options)
            : new OpenAIClient(new ApiKeyCredential(apiKey));

        var chatClient = client.GetChatClient(modelId);

        var messages = new List<OpenAI.Chat.ChatMessage>();

        // 添加系统提示
        if (!string.IsNullOrEmpty(systemPrompt))
        {
            messages.Add(new SystemChatMessage(systemPrompt));
        }

        // 添加历史消息
        foreach (var msg in history)
        {
            if (msg.Role.Equals("User", StringComparison.OrdinalIgnoreCase))
            {
                messages.Add(new UserChatMessage(msg.Content));
            }
            else if (msg.Role.Equals("Assistant", StringComparison.OrdinalIgnoreCase))
            {
                messages.Add(new AssistantChatMessage(msg.Content));
            }
        }

        // 添加当前消息
        messages.Add(new UserChatMessage(prompt));

        var chatOptions = new ChatCompletionOptions
        {
            Temperature = temperature,
            MaxOutputTokenCount = maxTokens
        };

        _logger.LogDebug("调用 API: BaseUrl={BaseUrl}, ModelId={ModelId}, MessageCount={Count}",
            baseUrl ?? "(default)", modelId, messages.Count);

        bool hasContent = false;

        await foreach (var update in chatClient.CompleteChatStreamingAsync(messages, chatOptions, cancellationToken))
        {
            foreach (var part in update.ContentUpdate)
            {
                if (!string.IsNullOrEmpty(part.Text))
                {
                    hasContent = true;
                    yield return part.Text;
                }
            }
        }

        if (!hasContent)
        {
            _logger.LogWarning("API 返回空内容");
            yield return "[警告] AI 返回了空内容，请检查 API 配置或模型是否正确";
        }
    }

    private async IAsyncEnumerable<string> CallClaudeApi(
        string apiKey,
        string baseUrl,
        string modelId,
        string prompt,
        IEnumerable<AppChatMessage> history,
        float temperature,
        int maxTokens,
        string? systemPrompt,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // Claude API 使用 HTTP 直接调用
        var url = string.IsNullOrEmpty(baseUrl) ? "https://api.anthropic.com" : baseUrl;

        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(url);
        httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
        httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

        var messages = new List<object>();

        foreach (var msg in history)
        {
            messages.Add(new { role = msg.Role.ToLower(), content = msg.Content });
        }
        messages.Add(new { role = "user", content = prompt });

        var requestBody = new
        {
            model = modelId,
            max_tokens = maxTokens,
            system = systemPrompt ?? "",
            messages = messages,
            stream = true
        };

        var jsonContent = System.Text.Json.JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

        using var request = new HttpRequestMessage(HttpMethod.Post, "/v1/messages");
        request.Content = content;

        HttpResponseMessage? response = null;
        string? errorMessage = null;

        try
        {
            response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            errorMessage = $"[错误] Claude API 调用失败: {ex.Message}";
        }

        if (errorMessage != null)
        {
            yield return errorMessage;
            yield break;
        }

        if (response == null)
        {
            yield return "[错误] 无法获取响应";
            yield break;
        }

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new System.IO.StreamReader(stream);

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrEmpty(line) || !line.StartsWith("data:")) continue;

            var data = line.Substring(5).Trim();
            if (data == "[DONE]") break;

            string? textContent = null;
            try
            {
                var json = System.Text.Json.JsonDocument.Parse(data);
                if (json.RootElement.TryGetProperty("delta", out var delta) &&
                    delta.TryGetProperty("text", out var text))
                {
                    textContent = text.GetString();
                }
            }
            catch { }

            if (!string.IsNullOrEmpty(textContent))
            {
                yield return textContent;
            }
        }
    }

    /// <summary>
    /// 流式发送消息（带思维链支持）
    /// </summary>
    public async Task<StreamResult> StreamMessageWithReasoningAsync(
        string prompt,
        IEnumerable<AppChatMessage> history,
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
        var reasoningBuilder = new System.Text.StringBuilder();

        // 从数据库获取模型和渠道信息
        var channels = await _channelRepository.GetAllAsync(cancellationToken);
        AiModel? aiModel = null;
        Channel? channel = null;

        foreach (var c in channels)
        {
            var model = c.Models.FirstOrDefault(m => m.ModelId == modelId && m.IsEnabled);
            if (model != null)
            {
                aiModel = model;
                channel = c;
                break;
            }
        }

        if (channel == null || aiModel == null)
        {
            await onContentChunk($"[模拟响应] 未找到模型 {modelId} 的配置。");
            result.Content = $"[模拟响应] 未找到模型 {modelId} 的配置。";
            return result;
        }

        var apiKey = _encryptionService.Decrypt(channel.ApiKey);
        if (string.IsNullOrEmpty(apiKey))
        {
            await onContentChunk("[错误] API Key 未配置");
            result.Content = "[错误] API Key 未配置";
            return result;
        }

        // DeepSeek 使用原生 HTTP 调用以获取 reasoning_content
        if (channel.Provider == Provider.DeepSeek)
        {
            await CallDeepSeekWithReasoningAsync(
                apiKey, channel.BaseUrl, modelId, prompt, history,
                temperature, maxTokens, systemPrompt,
                async (content) => { contentBuilder.Append(content); await onContentChunk(content); },
                async (reasoning) => { reasoningBuilder.Append(reasoning); if (onReasoningChunk != null) await onReasoningChunk(reasoning); },
                (inputTokens, outputTokens) => { result.InputTokens = inputTokens; result.OutputTokens = outputTokens; },
                cancellationToken);
        }
        else
        {
            // 其他 Provider 使用标准流式调用
            await foreach (var chunk in CallOpenAiCompatibleApi(apiKey, channel.BaseUrl, modelId, prompt, history, temperature, maxTokens, systemPrompt, cancellationToken))
            {
                contentBuilder.Append(chunk);
                await onContentChunk(chunk);
            }
        }

        result.Content = contentBuilder.ToString();
        result.ReasoningContent = reasoningBuilder.Length > 0 ? reasoningBuilder.ToString() : null;
        return result;
    }

    /// <summary>
    /// DeepSeek 原生调用（支持 reasoning_content）
    /// </summary>
    private async Task CallDeepSeekWithReasoningAsync(
        string apiKey,
        string baseUrl,
        string modelId,
        string prompt,
        IEnumerable<AppChatMessage> history,
        float temperature,
        int maxTokens,
        string? systemPrompt,
        Func<string, Task> onContent,
        Func<string, Task> onReasoning,
        Action<int?, int?> onUsage,
        CancellationToken cancellationToken)
    {
        var url = string.IsNullOrEmpty(baseUrl) ? "https://api.deepseek.com" : baseUrl;

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var messages = new List<object>();

        if (!string.IsNullOrEmpty(systemPrompt))
        {
            messages.Add(new { role = "system", content = systemPrompt });
        }

        foreach (var msg in history)
        {
            messages.Add(new { role = msg.Role.ToLower(), content = msg.Content });
        }
        messages.Add(new { role = "user", content = prompt });

        var requestBody = new
        {
            model = modelId,
            messages = messages,
            max_tokens = maxTokens,
            temperature = temperature,
            stream = true
        };

        var jsonContent = System.Text.Json.JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{url}/v1/chat/completions");
            request.Content = content;

            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new System.IO.StreamReader(stream);

            int? inputTokens = null;
            int? outputTokens = null;

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync(cancellationToken);
                if (string.IsNullOrEmpty(line) || !line.StartsWith("data:")) continue;

                var data = line.Substring(5).Trim();
                if (data == "[DONE]") break;

                try
                {
                    var json = System.Text.Json.JsonDocument.Parse(data);

                    // 解析 usage
                    if (json.RootElement.TryGetProperty("usage", out var usage))
                    {
                        if (usage.TryGetProperty("prompt_tokens", out var pt))
                            inputTokens = pt.GetInt32();
                        if (usage.TryGetProperty("completion_tokens", out var ct))
                            outputTokens = ct.GetInt32();
                    }

                    // 解析 choices
                    if (json.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                    {
                        var choice = choices[0];
                        if (choice.TryGetProperty("delta", out var delta))
                        {
                            // 解析 reasoning_content（DeepSeek 思维链）
                            if (delta.TryGetProperty("reasoning_content", out var reasoning))
                            {
                                var reasoningText = reasoning.GetString();
                                if (!string.IsNullOrEmpty(reasoningText))
                                {
                                    await onReasoning(reasoningText);
                                }
                            }

                            // 解析 content
                            if (delta.TryGetProperty("content", out var contentProp))
                            {
                                var contentText = contentProp.GetString();
                                if (!string.IsNullOrEmpty(contentText))
                                {
                                    await onContent(contentText);
                                }
                            }
                        }
                    }
                }
                catch (System.Text.Json.JsonException)
                {
                    // 忽略解析错误
                }
            }

            onUsage(inputTokens, outputTokens);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeepSeek API 调用失败");
            await onContent($"[错误] DeepSeek API 调用失败: {ex.Message}");
        }
    }
}
