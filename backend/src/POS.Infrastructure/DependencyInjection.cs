using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using POS.Application.Interfaces;
using POS.Application.Services;
using POS.Infrastructure.Auth;
using POS.Infrastructure.Persistence;
using POS.Infrastructure.Services;

namespace POS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("Default")));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IForecastingService, SimpleMovingAverageForecastingService>();

        // Payment orchestration: register one processor per PaymentMethod.
        // Swap StripeCardPaymentProcessor for AdyenCardPaymentProcessor, etc.,
        // without touching OrderService.
        services.AddScoped<IPaymentProcessor, CashPaymentProcessor>();
        services.AddScoped<IPaymentProcessor, StripeCardPaymentProcessor>();
        services.AddScoped<IPaymentProcessor, DigitalWalletPaymentProcessor>();

        services.AddScoped<AuthService>();
        services.AddScoped<ProductService>();
        services.AddScoped<OrderService>();

        return services;
    }
}
