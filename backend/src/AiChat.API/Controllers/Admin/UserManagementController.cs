using AiChat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiChat.API.Controllers.Admin;

/// <summary>
/// 用户管理接口（仅管理员）
/// </summary>
[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class UserManagementController : ControllerBase
{
    private readonly IAuthService _authService;

    public UserManagementController(IAuthService authService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
    }

    /// <summary>
    /// 封禁用户
    /// </summary>
    [HttpPost("{userId}/ban")]
    public async Task<IActionResult> BanUser(
        Guid userId,
        [FromBody] BanUserRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
            return BadRequest("封禁原因不能为空");

        await _authService.BanUserAsync(userId, request.Reason, cancellationToken);
        return Ok(new { message = "用户已封禁" });
    }

    /// <summary>
    /// 解封用户
    /// </summary>
    [HttpPost("{userId}/unban")]
    public async Task<IActionResult> UnbanUser(Guid userId, CancellationToken cancellationToken)
    {
        await _authService.UnbanUserAsync(userId, cancellationToken);
        return Ok(new { message = "用户已解封" });
    }

    /// <summary>
    /// 重置用户密码
    /// </summary>
    [HttpPut("{userId}/password")]
    public async Task<IActionResult> UpdateUserPassword(
        Guid userId,
        [FromBody] UpdatePasswordRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.NewPassword))
            return BadRequest("新密码不能为空");

        if (request.NewPassword.Length < 6)
            return BadRequest("密码长度至少6位");

        await _authService.UpdateUserPasswordAsync(userId, request.NewPassword, cancellationToken);
        return Ok(new { message = "密码已重置" });
    }
}

// Request DTOs
public record BanUserRequest
{
    public string Reason { get; init; } = string.Empty;
}

public record UpdatePasswordRequest
{
    public string NewPassword { get; init; } = string.Empty;
}
