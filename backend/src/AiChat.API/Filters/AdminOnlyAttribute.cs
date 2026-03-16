using System.Security.Claims;
using AiChat.Domain.Aggregates.UserAggregate;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AiChat.API.Filters;

/// <summary>
/// 管理员权限验证过滤器
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AdminOnlyAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // 从 Claims 获取角色
        var roleClaim = user.FindFirst("role") ?? user.FindFirst(ClaimTypes.Role);
        if (roleClaim == null)
        {
            context.Result = new ForbidResult();
            return;
        }

        // 检查是否是管理员
        if (!Enum.TryParse<UserRole>(roleClaim.Value, true, out var role) || role != UserRole.Admin)
        {
            context.Result = new ForbidResult();
            return;
        }
    }
}
