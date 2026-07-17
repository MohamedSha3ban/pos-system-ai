using Microsoft.Extensions.DependencyInjection;
using POS.Application.Modules.Identity.Services;

namespace POS.Application.Modules.Identity;

public static class IdentityApplicationModule
{
    public static IServiceCollection AddIdentityApplicationModule(this IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        return services;
    }
}
