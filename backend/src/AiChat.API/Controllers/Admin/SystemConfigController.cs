using AiChat.Application.DTOs;
using AiChat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiChat.API.Controllers.Admin;

/// <summary>
/// 系统配置管理接口（仅管理员）
/// </summary>
[ApiController]
[Route("api/admin/config")]
[Authorize(Roles = "Admin")]
public class SystemConfigController : ControllerBase
{
    private readonly ISystemConfigService _configService;

    public SystemConfigController(ISystemConfigService configService)
    {
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
    }

    /// <summary>
    /// 获取系统配置（管理员）
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<SystemConfigDto>> GetConfig(CancellationToken cancellationToken)
    {
        var config = await _configService.GetConfigAsync(cancellationToken);
        return Ok(config);
    }

    /// <summary>
    /// 更新系统配置
    /// </summary>
    [HttpPut]
    public async Task<ActionResult<SystemConfigDto>> UpdateConfig(
        [FromBody] UpdateSystemConfigRequest request,
        CancellationToken cancellationToken)
    {
        var config = await _configService.UpdateConfigAsync(request, cancellationToken);
        return Ok(config);
    }
}

/// <summary>
/// 公开配置接口（无需登录）
/// </summary>
[ApiController]
[Route("api/config")]
[AllowAnonymous]
public class PublicConfigController : ControllerBase
{
    private readonly ISystemConfigService _configService;

    public PublicConfigController(ISystemConfigService configService)
    {
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
    }

    /// <summary>
    /// 获取公开配置（无需登录）
    /// </summary>
    [HttpGet("public")]
    public async Task<ActionResult<PublicConfigDto>> GetPublicConfig(CancellationToken cancellationToken)
    {
        var config = await _configService.GetPublicConfigAsync(cancellationToken);
        return Ok(config);
    }
}
