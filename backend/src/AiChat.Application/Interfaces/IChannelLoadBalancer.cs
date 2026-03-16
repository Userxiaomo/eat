namespace AiChat.Application.Interfaces;

/// <summary>
/// 渠道负载均衡器接口
/// </summary>
public interface IChannelLoadBalancer
{
    /// <summary>
    /// 选择最优渠道
    /// </summary>
    /// <param name="modelId">模型 ID</param>
    /// <param name="userGroupId">用户分组 ID（可选）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>选中的渠道 ID，如果没有可用渠道返回 null</returns>
    Task<Guid?> SelectChannelAsync(string modelId, Guid? userGroupId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 标记渠道失败
    /// </summary>
    Task MarkChannelFailedAsync(Guid channelId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 标记渠道成功
    /// </summary>
    Task MarkChannelSuccessAsync(Guid channelId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取所有健康的渠道
    /// </summary>
    Task<IEnumerable<Guid>> GetHealthyChannelsAsync(CancellationToken cancellationToken = default);
}
