using AiChat.Application.DTOs;
using AiChat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiChat.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SearchController : ControllerBase
{
    private readonly IWebSearchService _searchService;
    private readonly ILogger<SearchController> _logger;

    public SearchController(IWebSearchService searchService, ILogger<SearchController> logger)
    {
        _searchService = searchService;
        _logger = logger;
    }

    /// <summary>
    /// 执行网络搜索
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WebSearchResult>>> Search(
        [FromQuery] string query,
        [FromQuery] int maxResults = 5,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest(new { error = "Query is required" });

        var results = await _searchService.SearchAsync(query, maxResults, cancellationToken);
        return Ok(results);
    }

    /// <summary>
    /// 检查搜索服务是否可用
    /// </summary>
    [HttpGet("status")]
    public async Task<ActionResult<object>> GetStatus(CancellationToken cancellationToken)
    {
        var available = await _searchService.IsAvailableAsync(cancellationToken);
        return Ok(new { available });
    }
}
