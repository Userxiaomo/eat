using AiChat.Application.DTOs;
using AiChat.Application.Interfaces;
using AiChat.Domain.Aggregates.SearchAggregate;
using AiChat.Infrastructure.Security;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace AiChat.Infrastructure.Search;

/// <summary>
/// Tavily 搜索服务实现
/// </summary>
public class TavilySearchService : IWebSearchService
{
    private readonly ISearchEngineConfigRepository _configRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<TavilySearchService> _logger;
    private readonly HttpClient _httpClient;

    public TavilySearchService(
        ISearchEngineConfigRepository configRepository,
        IEncryptionService encryptionService,
        IHttpClientFactory httpClientFactory,
        ILogger<TavilySearchService> logger)
    {
        _configRepository = configRepository ?? throw new ArgumentNullException(nameof(configRepository));
        _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClient = httpClientFactory.CreateClient("Tavily");
    }

    public async Task<IEnumerable<WebSearchResult>> SearchAsync(
        string query,
        int maxResults = 5,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Enumerable.Empty<WebSearchResult>();

        try
        {
            var config = await _configRepository.GetEnabledByTypeAsync(SearchEngineType.Tavily, cancellationToken);
            if (config == null)
            {
                // 尝试使用默认搜索引擎
                config = await _configRepository.GetDefaultAsync(cancellationToken);
                if (config == null)
                {
                    _logger.LogWarning("No search engine configured");
                    return Enumerable.Empty<WebSearchResult>();
                }
            }

            var apiKey = _encryptionService.Decrypt(config.ApiKey);
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("Search engine API key not configured");
                return Enumerable.Empty<WebSearchResult>();
            }

            var requestBody = new
            {
                api_key = apiKey,
                query = query,
                search_depth = "basic",
                max_results = maxResults
            };

            var response = await _httpClient.PostAsJsonAsync(
                "https://api.tavily.com/search",
                requestBody,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Tavily search failed: {StatusCode}", response.StatusCode);
                return Enumerable.Empty<WebSearchResult>();
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<TavilyResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result?.Results == null)
                return Enumerable.Empty<WebSearchResult>();

            return result.Results.Select(r => new WebSearchResult
            {
                Title = r.Title ?? string.Empty,
                Url = r.Url ?? string.Empty,
                Snippet = r.Content ?? string.Empty
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing web search");
            return Enumerable.Empty<WebSearchResult>();
        }
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        var config = await _configRepository.GetEnabledByTypeAsync(SearchEngineType.Tavily, cancellationToken);
        return config != null && !string.IsNullOrEmpty(config.ApiKey);
    }

    private class TavilyResponse
    {
        public List<TavilyResult>? Results { get; set; }
    }

    private class TavilyResult
    {
        public string? Title { get; set; }
        public string? Url { get; set; }
        public string? Content { get; set; }
    }
}
