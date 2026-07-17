using Stripe;
using POS.Application.Modules.Orders.Interfaces;
using POS.Domain.Modules.Orders.Enums;

namespace POS.Infrastructure.Modules.Payments;

/// <summary>
/// Real Stripe integration for card-present / card-not-present tenders, via the
/// PaymentIntents API (https://stripe.com/docs/api/payment_intents). For an actual
/// card-present flow, pair this with Stripe Terminal on the client (web/mobile) to
/// collect the card and produce a PaymentMethod id, which is passed in as `paymentToken`.
/// </summary>
public class StripeCardPaymentProcessor : IPaymentProcessor
{
    public virtual PaymentMethod SupportedMethod => PaymentMethod.CardPresent;

    public async Task<PaymentChargeResult> ChargeAsync(Guid tenantId, decimal amount, string currency, string? paymentToken, CancellationToken ct = default)
    {
        try
        {
            var service = new PaymentIntentService();
            var options = new PaymentIntentCreateOptions
            {
                Amount = ToMinorUnits(amount),
                Currency = currency,
                PaymentMethod = paymentToken,
                Confirm = !string.IsNullOrEmpty(paymentToken),
                PaymentMethodTypes = new List<string> { "card" },
                Metadata = new Dictionary<string, string> { { "tenantId", tenantId.ToString() } }
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
                Amount = ToMinorUnits(amount)
            }, cancellationToken: ct);

            return new PaymentChargeResult(refund.Status == "succeeded", refund.Id, null);
        }
        catch (StripeException ex)
        {
            return new PaymentChargeResult(false, null, ex.StripeError?.Message ?? ex.Message);
        }
    }

    protected static long ToMinorUnits(decimal amount) => (long)Math.Round(amount * 100, MidpointRounding.AwayFromZero);
}
