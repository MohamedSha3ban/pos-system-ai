using Microsoft.Extensions.DependencyInjection;
using POS.Application.Modules.Identity.Services;

namespace POS.Application.Modules.Identity;

public static class IdentityApplicationModule
{
    public static IServiceCollection AddIdentityApplicationModule(this IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        services.AddScoped<UserService>();
        services.AddScoped<RoleService>();
        services.AddScoped<TenantService>();
        return services;
    }
}
