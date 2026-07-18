using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using POS.Application.Common.Interfaces;
using POS.Application.Modules.Catalog;
using POS.Application.Modules.Identity;
using POS.Application.Modules.Orders;
using POS.Application.Modules.Storefront;
using POS.Infrastructure.Modules.Identity;
using POS.Infrastructure.Modules.Insights;
using POS.Infrastructure.Modules.Payments;
using POS.Infrastructure.Persistence;

namespace POS.Infrastructure;

/// <summary>
/// Composition root for the whole backend. Each bounded-context module exposes its own
/// AddXModule() extension (Application layer: use-cases/services; Infrastructure layer:
/// concrete implementations) so modules stay independently registerable/testable and
/// could be extracted into separate services later with minimal rewiring here.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // CQRS-lite: two DbContexts, two connection strings. "Read" falls back to "Write"
        // so local dev works out of the box against a single database with no replica --
        // point "Read" at an actual replica host in production to get the scaling benefit.
        var writeConnectionString = config.GetConnectionString("Write");
        var readConnectionString = config.GetConnectionString("Read") ?? writeConnectionString;

        services.AddDbContext<WriteDbContext>(options => options.UseNpgsql(writeConnectionString));
        services.AddDbContext<ReadDbContext>(options => options.UseNpgsql(readConnectionString));

        services.AddScoped<IWriteDbContext>(sp => sp.GetRequiredService<WriteDbContext>());
        services.AddScoped<IReadDbContext>(sp => sp.GetRequiredService<ReadDbContext>());

        // Application-layer modules (use-cases)
        services.AddIdentityApplicationModule();
        services.AddCatalogApplicationModule();
        services.AddOrdersApplicationModule();
        services.AddStorefrontApplicationModule();

        // Infrastructure-layer modules (concrete implementations)
        services.AddIdentityInfrastructureModule();
        services.AddPaymentsInfrastructureModule(config);
        services.AddInsightsInfrastructureModule();

        return services;
    }
}
