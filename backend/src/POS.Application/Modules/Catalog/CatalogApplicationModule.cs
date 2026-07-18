using Microsoft.Extensions.DependencyInjection;
using POS.Application.Modules.Catalog.Services;

namespace POS.Application.Modules.Catalog;

public static class CatalogApplicationModule
{
    public static IServiceCollection AddCatalogApplicationModule(this IServiceCollection services)
    {
        services.AddScoped<ProductService>();
        services.AddScoped<CategoryService>();
        services.AddScoped<InventoryService>();
        return services;
    }
}
