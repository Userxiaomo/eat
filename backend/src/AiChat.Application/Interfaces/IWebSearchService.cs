using AiChat.Application.DTOs;
using AiChat.Domain.Aggregates.SearchAggregate;

namespace AiChat.Application.Interfaces;

public interface IWebSearchService
{
    /// <summary>
    /// 执行网络搜索
    /// </summary>
    Task<IEnumerable<WebSearchResult>> SearchAsync(
        string query,
        int maxResults = 5,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查搜索服务是否可用
    /// </summary>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Web 搜索服务工厂接口
/// </summary>
public interface IWebSearchServiceFactory
{
    /// <summary>
    /// 根据搜索引擎类型获取搜索服务
    /// </summary>
    IWebSearchService GetSearchService(SearchEngineType engineType);

    /// <summary>
    /// 获取默认搜索服务
    /// </summary>
    Task<IWebSearchService> GetDefaultSearchServiceAsync(CancellationToken cancellationToken = default);
}
