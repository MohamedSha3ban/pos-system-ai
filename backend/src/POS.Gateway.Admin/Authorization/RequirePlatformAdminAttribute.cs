using Microsoft.AspNetCore.Mvc.Filters;

namespace POS.Gateway.Admin.Authorization;

/// <summary>Gates the platform-admin-only Tenants screen. See User.IsPlatformAdmin.</summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequirePlatformAdminAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (user.Identity?.IsAuthenticated != true)
        {
            context.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedResult();
            return;
        }

        var isPlatformAdmin = user.Claims.Any(c => c.Type == "isPlatformAdmin" && c.Value == "true");
        if (!isPlatformAdmin)
            context.Result = new Microsoft.AspNetCore.Mvc.ForbidResult();
    }
}
