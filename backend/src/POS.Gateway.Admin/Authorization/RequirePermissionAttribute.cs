using Microsoft.AspNetCore.Mvc.Filters;

namespace POS.Gateway.Admin.Authorization;

/// <summary>
/// Checks the caller's JWT for a "permission" claim matching the required code (see
/// Domain.Common.Permissions). Staff JWTs carry one "permission" claim per granted
/// permission, computed at login from the user's assigned roles (see AuthService).
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequirePermissionAttribute : Attribute, IAuthorizationFilter
{
    private readonly string _permission;
    public RequirePermissionAttribute(string permission) => _permission = permission;

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (user.Identity?.IsAuthenticated != true)
        {
            context.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedResult();
            return;
        }

        var hasPermission = user.Claims.Any(c => c.Type == "permission" && c.Value == _permission);
        if (!hasPermission)
            context.Result = new Microsoft.AspNetCore.Mvc.ForbidResult();
    }
}
