using AiChat.Domain.Common;

namespace AiChat.Domain.Aggregates.UserAggregate;

/// <summary>
/// Token 使用报告（按日期/用户/模型统计）
/// </summary>
public class UsageReport : Entity<Guid>
{
    /// <summary>
    /// 统计日期
    /// </summary>
    public DateOnly Date { get; private set; }

    /// <summary>
    /// 用户 ID
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// 模型 ID
    /// </summary>
    public Guid ModelId { get; private set; }

    /// <summary>
    /// Provider ID（渠道 ID）
    /// </summary>
    public Guid ProviderId { get; private set; }

    /// <summary>
    /// 输入 Token 数量
    /// </summary>
    public int InputTokens { get; private set; }

    /// <summary>
    /// 输出 Token 数量
    /// </summary>
    public int OutputTokens { get; private set; }

    /// <summary>
    /// 总 Token 数量
    /// </summary>
    public int TotalTokens => InputTokens + OutputTokens;

    private UsageReport() { }

    public UsageReport(Guid id, DateOnly date, Guid userId, Guid modelId, Guid providerId) : base(id)
    {
        Date = date;
        UserId = userId;
        ModelId = modelId;
        ProviderId = providerId;
    }

    public void AddTokenUsage(int inputTokens, int outputTokens)
    {
        InputTokens += inputTokens;
        OutputTokens += outputTokens;
    }
}
