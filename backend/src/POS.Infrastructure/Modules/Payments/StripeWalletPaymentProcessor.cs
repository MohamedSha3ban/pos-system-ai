using Stripe;
using POS.Application.Modules.Orders.Interfaces;
using POS.Domain.Modules.Orders.Enums;

namespace POS.Infrastructure.Modules.Payments;

/// <summary>
/// Apple Pay / Google Pay both flow through Stripe's PaymentIntents API the same way
/// card does (Stripe normalizes wallets under the same endpoint) -- this class is
/// registered twice in DI, once per PaymentMethod, so OrderService's per-method lookup
/// still works cleanly.
/// </summary>
public class StripeWalletPaymentProcessor : IPaymentProcessor
{
    private readonly PaymentMethod _method;
    public StripeWalletPaymentProcessor(PaymentMethod method) => _method = method;

    public PaymentMethod SupportedMethod => _method;

    public async Task<PaymentChargeResult> ChargeAsync(Guid tenantId, decimal amount, string currency, string? paymentToken, CancellationToken ct = default)
    {
        try
        {
            var service = new PaymentIntentService();
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)Math.Round(amount * 100, MidpointRounding.AwayFromZero),
                Currency = currency,
                PaymentMethod = paymentToken,
                Confirm = !string.IsNullOrEmpty(paymentToken),
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true },
                Metadata = new Dictionary<string, string> { { "tenantId", tenantId.ToString() }, { "wallet", _method.ToString() } }
            };

            var intent = await service.CreateAsync(options, cancellationToken: ct);
            var success = intent.Status is "succeeded" or "requires_capture";

            return new PaymentChargeResult(success, intent.Id, success ? null : $"Stripe status: {intent.Status}", intent.ClientSecret);
        }
        catch (StripeException ex)
        {
            return new PaymentChargeResult(false, null, ex.StripeError?.Message ?? ex.Message);
        }
    }

    public async Task<PaymentChargeResult> RefundAsync(string processorReference, decimal amount, CancellationToken ct = default)
    {
        try
        {
            var service = new RefundService();
            var refund = await service.CreateAsync(new RefundCreateOptions
            {
                PaymentIntent = processorReference,
                Amount = (long)Math.Round(amount * 100, MidpointRounding.AwayFromZero)
            }, cancellationToken: ct);

            return new PaymentChargeResult(refund.Status == "succeeded", refund.Id, null);
        }
        catch (StripeException ex)
        {
            return new PaymentChargeResult(false, null, ex.StripeError?.Message ?? ex.Message);
        }
    }
}
