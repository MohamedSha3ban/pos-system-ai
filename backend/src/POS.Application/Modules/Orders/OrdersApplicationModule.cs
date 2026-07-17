using Microsoft.Extensions.DependencyInjection;
using POS.Application.Modules.Orders.Services;

namespace POS.Application.Modules.Orders;

public static class OrdersApplicationModule
{
    public static IServiceCollection AddOrdersApplicationModule(this IServiceCollection services)
    {
        services.AddScoped<OrderService>();
        return services;
    }
}
