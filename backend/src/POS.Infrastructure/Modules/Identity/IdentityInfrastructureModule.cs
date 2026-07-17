using Microsoft.Extensions.DependencyInjection;
using POS.Application.Modules.Identity.Interfaces;

namespace POS.Infrastructure.Modules.Identity;

public static class IdentityInfrastructureModule
{
    public static IServiceCollection AddIdentityInfrastructureModule(this IServiceCollection services)
    {
        services.AddScoped<ITokenService, JwtTokenService>();
        return services;
    }
}
