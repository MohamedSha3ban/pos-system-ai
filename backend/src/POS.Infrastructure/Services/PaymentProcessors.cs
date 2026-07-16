using POS.Application.Interfaces;
using POS.Domain.Enums;

namespace POS.Infrastructure.Services;

/// <summary>
/// Cash needs no external processor -- always "succeeds" once the cashier confirms.
/// </summary>
public class CashPaymentProcessor : IPaymentProcessor
{
    public PaymentMethod SupportedMethod => PaymentMethod.Cash;

    public Task<PaymentChargeResult> ChargeAsync(Guid tenantId, decimal amount, string currency, string? paymentToken, CancellationToken ct = default)
        => Task.FromResult(new PaymentChargeResult(true, ProcessorReference: $"CASH-{Guid.NewGuid():N}", null));

    public Task<PaymentChargeResult> RefundAsync(string processorReference, decimal amount, CancellationToken ct = default)
        => Task.FromResult(new PaymentChargeResult(true, processorReference, null));
}

/// <summary>
/// Stub for card-present/tap-to-pay. Replace the body with a call to Stripe Terminal's
/// PaymentIntent API (https://stripe.com/docs/terminal) -- kept as a stub here since
/// this sandbox has no network access to Stripe and no real API keys.
/// </summary>
public class StripeCardPaymentProcessor : IPaymentProcessor
{
    public PaymentMethod SupportedMethod => PaymentMethod.CardPresent;

    public async Task<PaymentChargeResult> ChargeAsync(Guid tenantId, decimal amount, string currency, string? paymentToken, CancellationToken ct = default)
    {
        // TODO: call Stripe PaymentIntents API here using the Stripe.net SDK.
        await Task.Delay(50, ct); // simulate network latency
        return new PaymentChargeResult(true, ProcessorReference: $"pi_{Guid.NewGuid():N}", null);
    }

    public Task<PaymentChargeResult> RefundAsync(string processorReference, decimal amount, CancellationToken ct = default)
        => Task.FromResult(new PaymentChargeResult(true, processorReference, null));
}

/// <summary>
/// Digital wallets route through the same processor as card-present in most setups
/// (Stripe/Adyen normalize Apple Pay/Google Pay under one API) -- separated here so
/// a region-specific wallet processor can be swapped in independently.
/// </summary>
public class DigitalWalletPaymentProcessor : IPaymentProcessor
{
    public PaymentMethod SupportedMethod => PaymentMethod.ApplePay;

    public async Task<PaymentChargeResult> ChargeAsync(Guid tenantId, decimal amount, string currency, string? paymentToken, CancellationToken ct = default)
    {
        await Task.Delay(50, ct);
        return new PaymentChargeResult(true, ProcessorReference: $"wallet_{Guid.NewGuid():N}", null);
    }

    public Task<PaymentChargeResult> RefundAsync(string processorReference, decimal amount, CancellationToken ct = default)
        => Task.FromResult(new PaymentChargeResult(true, processorReference, null));
}
