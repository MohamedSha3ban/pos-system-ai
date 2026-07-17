using Microsoft.Extensions.DependencyInjection;
using POS.Application.Modules.Insights.Interfaces;

namespace POS.Infrastructure.Modules.Insights;

public static class InsightsInfrastructureModule
{
    public static IServiceCollection AddInsightsInfrastructureModule(this IServiceCollection services)
    {
        services.AddScoped<IForecastingService, SimpleMovingAverageForecastingService>();
        return services;
    }
}
