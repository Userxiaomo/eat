using AiChat.Application.DTOs;
using AiChat.Application.Interfaces;
using AiChat.Domain.Aggregates.SearchAggregate;
using AiChat.Infrastructure.Security;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace AiChat.Infrastructure.Search;

/// <summary>
/// Jina 搜索服务实现
/// </summary>
public class JinaSearchService : IWebSearchService
{
    private readonly ISearchEngineConfigRepository _configRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<JinaSearchService> _logger;
    private readonly HttpClient _httpClient;

    private const string JinaApiUrl = "https://s.jina.ai/";

    public JinaSearchService(
        ISearchEngineConfigRepository configRepository,
        IEncryptionService encryptionService,
        IHttpClientFactory httpClientFactory,
        ILogger<JinaSearchService> logger)
    {
        _configRepository = configRepository ?? throw new ArgumentNullException(nameof(configRepository));
        _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClient = httpClientFactory.CreateClient("Jina");
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
            var config = await _configRepository.GetEnabledByTypeAsync(SearchEngineType.Jina, cancellationToken);
            if (config == null)
            {
                _logger.LogWarning("Jina search engine not configured");
                return Enumerable.Empty<WebSearchResult>();
            }

            var apiKey = _encryptionService.Decrypt(config.ApiKey);
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("Jina API key not configured");
                return Enumerable.Empty<WebSearchResult>();
            }

            // Jina Reader API 使用 URL 编码的查询
            var encodedQuery = Uri.EscapeDataString(query);
            var request = new HttpRequestMessage(HttpMethod.Get, $"{JinaApiUrl}{encodedQuery}");
            request.Headers.Add("Authorization", $"Bearer {apiKey}");
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("X-Return-Format", "json");

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Jina search failed: {StatusCode}", response.StatusCode);
                return Enumerable.Empty<WebSearchResult>();
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<JinaResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result?.Data == null)
                return Enumerable.Empty<WebSearchResult>();

            return result.Data
                .Take(maxResults)
                .Select(r => new WebSearchResult
                {
                    Title = r.Title ?? string.Empty,
                    Url = r.Url ?? string.Empty,
                    Snippet = r.Description ?? r.Content ?? string.Empty
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing Jina search");
            return Enumerable.Empty<WebSearchResult>();
        }
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        var config = await _configRepository.GetEnabledByTypeAsync(SearchEngineType.Jina, cancellationToken);
        return config != null && !string.IsNullOrEmpty(config.ApiKey);
    }

    private class JinaResponse
    {
        public List<JinaResult>? Data { get; set; }
    }

    private class JinaResult
    {
        public string? Title { get; set; }
        public string? Url { get; set; }
        public string? Description { get; set; }
        public string? Content { get; set; }
    }
}
