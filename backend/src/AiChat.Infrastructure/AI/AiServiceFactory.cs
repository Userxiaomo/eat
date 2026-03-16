using AiChat.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AiChat.Infrastructure.AI;

public class AiServiceFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AiServiceFactory> _logger;

    public AiServiceFactory(IServiceProvider serviceProvider, ILogger<AiServiceFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public IAiService GetService(string modelId)
    {
        // 统一使用动态 AI 服务，从数据库读取渠道配置
        _logger.LogDebug("获取模型 {ModelId} 的 AI 服务", modelId);
        return _serviceProvider.GetRequiredService<DynamicAiService>();
    }
}
