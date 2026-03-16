using AiChat.Application.DTOs;
using AiChat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiChat.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class McpController : ControllerBase
{
    private readonly IMcpService _mcpService;
    private readonly ILogger<McpController> _logger;

    public McpController(IMcpService mcpService, ILogger<McpController> logger)
    {
        _mcpService = mcpService;
        _logger = logger;
    }

    /// <summary>
    /// 获取所有 MCP 服务器
    /// </summary>
    [HttpGet("servers")]
    public async Task<ActionResult<IEnumerable<McpServerDto>>> GetServers(CancellationToken cancellationToken)
    {
        var servers = await _mcpService.GetAllServersAsync(cancellationToken);
        return Ok(servers);
    }

    /// <summary>
    /// 获取单个 MCP 服务器
    /// </summary>
    [HttpGet("servers/{id:guid}")]
    public async Task<ActionResult<McpServerDto>> GetServer(Guid id, CancellationToken cancellationToken)
    {
        var server = await _mcpService.GetServerByIdAsync(id, cancellationToken);
        if (server == null)
            return NotFound();

        return Ok(server);
    }

    /// <summary>
    /// 创建 MCP 服务器
    /// </summary>
    [HttpPost("servers")]
    public async Task<ActionResult<McpServerDto>> CreateServer(CreateMcpServerRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var server = await _mcpService.CreateServerAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetServer), new { id = server.Id }, server);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 更新 MCP 服务器
    /// </summary>
    [HttpPut("servers/{id:guid}")]
    public async Task<ActionResult<McpServerDto>> UpdateServer(Guid id, UpdateMcpServerRequest request, CancellationToken cancellationToken)
    {
        var server = await _mcpService.UpdateServerAsync(id, request, cancellationToken);
        if (server == null)
            return NotFound();

        return Ok(server);
    }

    /// <summary>
    /// 删除 MCP 服务器
    /// </summary>
    [HttpDelete("servers/{id:guid}")]
    public async Task<IActionResult> DeleteServer(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _mcpService.DeleteServerAsync(id, cancellationToken);
        if (!deleted)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// 获取已启用的 MCP 服务器列表
    /// </summary>
    [HttpGet("servers/enabled")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<McpServerDto>>> GetEnabledServers(CancellationToken cancellationToken)
    {
        var servers = await _mcpService.GetEnabledServersAsync(cancellationToken);
        return Ok(servers);
    }
}
