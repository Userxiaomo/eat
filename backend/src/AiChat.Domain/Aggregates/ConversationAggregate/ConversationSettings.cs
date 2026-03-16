using AiChat.Domain.Common;

namespace AiChat.Domain.Aggregates.ConversationAggregate;

/// <summary>
/// 会话设置（值对象）
/// </summary>
public class ConversationSettings : ValueObject
{
    public double Temperature { get; init; } = 0.7;
    public int MaxTokens { get; init; } = 2000;
    public string? SystemPrompt { get; init; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Temperature;
        yield return MaxTokens;
        yield return SystemPrompt;
    }

    public static ConversationSettings Default => new()
    {
        Temperature = 0.7,
        MaxTokens = 2000,
        SystemPrompt = null
    };
}
