using AiChat.Application.Interfaces;
using AiChat.Domain.Aggregates.SearchAggregate;
using Microsoft.Extensions.DependencyInjection;

namespace AiChat.Infrastructure.Search;

/// <summary>
/// Web 搜索服务工厂
/// </summary>
public class WebSearchServiceFactory : IWebSearchServiceFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISearchEngineConfigRepository _configRepository;

    public WebSearchServiceFactory(
        IServiceProvider serviceProvider,
        ISearchEngineConfigRepository configRepository)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _configRepository = configRepository ?? throw new ArgumentNullException(nameof(configRepository));
    }

    /// <summary>
    /// 根据搜索引擎类型获取搜索服务
    /// </summary>
    public IWebSearchService GetSearchService(SearchEngineType engineType)
    {
        return engineType switch
        {
            SearchEngineType.Tavily => _serviceProvider.GetRequiredService<TavilySearchService>(),
            SearchEngineType.Jina => _serviceProvider.GetRequiredService<JinaSearchService>(),
            // 可扩展其他搜索引擎
            _ => _serviceProvider.GetRequiredService<TavilySearchService>() // 默认使用 Tavily
        };
    }

    /// <summary>
    /// 获取默认搜索服务
    /// </summary>
    public async Task<IWebSearchService> GetDefaultSearchServiceAsync(CancellationToken cancellationToken = default)
    {
        var defaultConfig = await _configRepository.GetDefaultAsync(cancellationToken);
        if (defaultConfig != null)
        {
            return GetSearchService(defaultConfig.EngineType);
        }

        // 尝试获取任意启用的搜索引擎
        var enabledConfig = await _configRepository.GetFirstEnabledAsync(cancellationToken);
        if (enabledConfig != null)
        {
            return GetSearchService(enabledConfig.EngineType);
        }

        // 默认返回 Tavily
        return _serviceProvider.GetRequiredService<TavilySearchService>();
    }
}
