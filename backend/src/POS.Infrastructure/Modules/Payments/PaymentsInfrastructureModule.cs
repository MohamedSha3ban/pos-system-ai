using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Stripe;
using POS.Application.Modules.Orders.Interfaces;
using POS.Domain.Modules.Orders.Enums;

namespace POS.Infrastructure.Modules.Payments;

public static class PaymentsInfrastructureModule
{
    public static IServiceCollection AddPaymentsInfrastructureModule(this IServiceCollection services, IConfiguration config)
    {
        var stripeOptions = config.GetSection(StripeOptions.SectionName).Get<StripeOptions>() ?? new StripeOptions();
        services.Configure<StripeOptions>(config.GetSection(StripeOptions.SectionName));

        // Stripe.net reads its API key from this static property.
        StripeConfiguration.ApiKey = stripeOptions.SecretKey;

        services.AddScoped<IPaymentProcessor, CashPaymentProcessor>();
        services.AddScoped<IPaymentProcessor, StripeCardPaymentProcessor>();
        services.AddScoped<IPaymentProcessor>(_ => new StripeWalletPaymentProcessor(PaymentMethod.ApplePay));
        services.AddScoped<IPaymentProcessor>(_ => new StripeWalletPaymentProcessor(PaymentMethod.GooglePay));

        // TODO: register a QrBankTransfer processor per target region (e.g. Moyasar/HyperPay
        // for Saudi mada/STC Pay) and a BuyNowPayLater processor (Tabby/Tamara/Klarna) --
        // both plug in the same way, implementing IPaymentProcessor.

        return services;
    }
}
