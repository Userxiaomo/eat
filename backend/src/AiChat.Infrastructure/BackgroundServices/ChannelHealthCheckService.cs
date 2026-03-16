using AiChat.Domain.Aggregates.ChannelAggregate;
using AiChat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AiChat.Infrastructure.BackgroundServices;

/// <summary>
/// 渠道健康检查后台服务
/// 定期检查失败的渠道，自动恢复已超时的不健康渠道
/// </summary>
public class ChannelHealthCheckService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ChannelHealthCheckService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5);  // 每5分钟检查一次
    private readonly TimeSpan _autoRecoveryTimeout = TimeSpan.FromMinutes(10);  // 失败后10分钟自动恢复

    public ChannelHealthCheckService(
        IServiceProvider serviceProvider,
        ILogger<ChannelHealthCheckService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("渠道健康检查服务已启动");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndRecoverChannelsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "渠道健康检查过程中发生错误");
            }

            // 等待下次检查
            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("渠道健康检查服务已停止");
    }

    private async Task CheckAndRecoverChannelsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AiChatDbContext>();
        var channelRepository = scope.ServiceProvider.GetRequiredService<IChannelRepository>();

        // 查找所有不健康的渠道
        var unhealthyChannels = await context.Set<Channel>()
            .Where(c => !c.IsHealthy && c.LastFailedAt != null)
            .ToListAsync(cancellationToken);

        if (!unhealthyChannels.Any())
            return;

        var now = DateTime.UtcNow;
        var recoveredCount = 0;

        foreach (var channel in unhealthyChannels)
        {
            // 检查是否超过自动恢复时间
            if (channel.LastFailedAt.HasValue &&
                (now - channel.LastFailedAt.Value) > _autoRecoveryTimeout)
            {
                channel.MarkHealthy();
                recoveredCount++;

                _logger.LogInformation(
                    "渠道 {ChannelName} (ID: {ChannelId}) 已自动恢复健康状态（失败时间：{FailedAt}）",
                    channel.Name,
                    channel.Id,
                    channel.LastFailedAt.Value);
            }
        }

        if (recoveredCount > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("共恢复了 {Count} 个渠道的健康状态", recoveredCount);
        }
    }
}
