using Microsoft.Extensions.DependencyInjection;
using POS.Application.Modules.Storefront.Services;

namespace POS.Application.Modules.Storefront;

public static class StorefrontApplicationModule
{
    public static IServiceCollection AddStorefrontApplicationModule(this IServiceCollection services)
    {
        services.AddScoped<CustomerAuthService>();
        return services;
    }
}
