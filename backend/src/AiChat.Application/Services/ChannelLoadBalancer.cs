using AiChat.Application.Interfaces;
using AiChat.Domain.Aggregates.ChannelAggregate;

namespace AiChat.Application.Services;

/// <summary>
/// 渠道负载均衡器服务
/// </summary>
public class ChannelLoadBalancer : IChannelLoadBalancer
{
    private readonly IChannelRepository _channelRepository;
    private readonly IAiModelRepository _modelRepository;

    public ChannelLoadBalancer(
        IChannelRepository channelRepository,
        IAiModelRepository modelRepository)
    {
        _channelRepository = channelRepository ?? throw new ArgumentNullException(nameof(channelRepository));
        _modelRepository = modelRepository ?? throw new ArgumentNullException(nameof(modelRepository));
    }

    /// <summary>
    /// 选择最优渠道
    /// 算法：
    /// 1. 过滤：IsEnabled && IsHealthy && 支持该模型
    /// 2. 按 Priority 分组排序（数字越小优先级越高）
    /// 3. 同一 Priority 内按 Weight 加权随机选择
    /// </summary>
    public async Task<Guid?> SelectChannelAsync(
        string modelId,
        Guid? userGroupId = null,
        CancellationToken cancellationToken = default)
    {
        // 1. 获取所有渠道
        var allChannels = await _channelRepository.GetAllAsync(cancellationToken);

        // 2. 过滤可用渠道：启用 && 健康 && 支持该模型
        var availableChannels = allChannels
            .Where(c => c.IsEnabled && c.IsHealthy)
            .Where(c => c.Models.Any(m => m.ModelId == modelId && m.IsEnabled))
            .ToList();

        if (!availableChannels.Any())
            return null;

        // 3. 按优先级分组
        var groupedByPriority = availableChannels
            .GroupBy(c => c.Priority)
            .OrderBy(g => g.Key)  // 优先级数字越小越优先
            .ToList();

        // 4. 选择最高优先级组
        var highestPriorityGroup = groupedByPriority.First().ToList();

        // 5. 如果该组只有一个渠道，直接返回
        if (highestPriorityGroup.Count == 1)
            return highestPriorityGroup[0].Id;

        // 6. 按权重加权随机选择
        var selected = WeightedRandomSelect(highestPriorityGroup);
        return selected?.Id;
    }

    /// <summary>
    /// 标记渠道失败
    /// </summary>
    public async Task MarkChannelFailedAsync(Guid channelId, CancellationToken cancellationToken = default)
    {
        var channel = await _channelRepository.GetByIdAsync(channelId, cancellationToken);
        if (channel == null)
            return;

        channel.MarkFailed();
        await _channelRepository.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 标记渠道成功
    /// </summary>
    public async Task MarkChannelSuccessAsync(Guid channelId, CancellationToken cancellationToken = default)
    {
        var channel = await _channelRepository.GetByIdAsync(channelId, cancellationToken);
        if (channel == null)
            return;

        // 如果之前是不健康状态，标记为健康
        if (!channel.IsHealthy)
        {
            channel.MarkHealthy();
            await _channelRepository.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// 获取所有健康的渠道
    /// </summary>
    public async Task<IEnumerable<Guid>> GetHealthyChannelsAsync(CancellationToken cancellationToken = default)
    {
        var channels = await _channelRepository.GetAllAsync(cancellationToken);
        return channels
            .Where(c => c.IsEnabled && c.IsHealthy)
            .Select(c => c.Id)
            .ToList();
    }

    /// <summary>
    /// 加权随机选择
    /// </summary>
    private static Channel? WeightedRandomSelect(List<Channel> channels)
    {
        if (!channels.Any())
            return null;

        // 计算总权重
        var totalWeight = channels.Sum(c => c.Weight);

        // 生成随机数 [0, totalWeight)
        var randomValue = Random.Shared.Next(totalWeight);

        // 找到对应的渠道
        var currentWeight = 0;
        foreach (var channel in channels)
        {
            currentWeight += channel.Weight;
            if (randomValue < currentWeight)
                return channel;
        }

        // 理论上不会到这里，但为了安全返回第一个
        return channels.First();
    }
}
