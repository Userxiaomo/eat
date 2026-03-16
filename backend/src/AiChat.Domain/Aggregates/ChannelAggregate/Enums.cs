namespace AiChat.Domain.Aggregates.ChannelAggregate;

/// <summary>
/// AI 服务提供商
/// </summary>
public enum Provider
{
    OpenAI = 0,
    Claude = 1,
    Azure = 2,
    DeepSeek = 3,
    Qwen = 4,           // 通义千问（阿里百炼）
    Gemini = 5,         // Google Gemini
    Moonshot = 6,       // 月之暗面
    VolcEngine = 7,     // 火山方舟（豆包）
    Zhipu = 8,          // 智谱 GLM
    Baidu = 9,          // 百度千帆
    Tencent = 10,       // 腾讯混元
    OpenRouter = 11,    // OpenRouter
    Grok = 12,          // xAI Grok
    Ollama = 13,        // Ollama 本地
    SiliconFlow = 14,   // 硅基流动
    Custom = 99         // 自定义
}

/// <summary>
/// API 风格（不同厂商的 API 格式）
/// </summary>
public enum ApiStyle
{
    /// <summary>
    /// OpenAI 兼容格式（大多数厂商）
    /// </summary>
    OpenAI = 0,

    /// <summary>
    /// OpenAI Response API 格式
    /// </summary>
    OpenAIResponse = 1,

    /// <summary>
    /// Claude/Anthropic 格式
    /// </summary>
    Claude = 2,

    /// <summary>
    /// Google Gemini 格式
    /// </summary>
    Gemini = 3
}

/// <summary>
/// 模型类型
/// </summary>
public enum ModelType
{
    Chat = 0,       // 聊天模型
    Image = 1,      // 图片生成模型
    Embedding = 2   // 嵌入式模型
}
