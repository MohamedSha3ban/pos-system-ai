using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using POS.Infrastructure.Modules.Payments;

namespace POS.Gateway.Admin.Controllers.Payments;

/// <summary>
/// Receives async payment status updates from Stripe. Register this URL in the Stripe
/// dashboard as https://your-admin-gateway-host/api/payments/stripe/webhook -- lives in
/// the Admin gateway since payment operations/reconciliation is a back-office concern,
/// even though the *payment itself* might have originated from the Ecommerce gateway.
/// Not [Authorize] -- Stripe signs the payload instead; verified below.
/// </summary>
[ApiController]
[Route("api/payments/stripe")]
public class StripeWebhookController : ControllerBase
{
    private readonly StripeOptions _options;
    private readonly ILogger<StripeWebhookController> _logger;

    public StripeWebhookController(IOptions<StripeOptions> options, ILogger<StripeWebhookController> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook()
    {
        var json = await new StreamReader(Request.Body).ReadToEndAsync();

        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                _options.WebhookSecret);

            switch (stripeEvent.Type)
            {
                case "payment_intent.succeeded":
                    var succeeded = (PaymentIntent)stripeEvent.Data.Object;
                    _logger.LogInformation("PaymentIntent {Id} succeeded", succeeded.Id);
                    break;

                case "payment_intent.payment_failed":
                    var failed = (PaymentIntent)stripeEvent.Data.Object;
                    _logger.LogWarning("PaymentIntent {Id} failed", failed.Id);
                    break;

                case "charge.refunded":
                    var refunded = (Charge)stripeEvent.Data.Object;
                    _logger.LogInformation("Charge {Id} refunded", refunded.Id);
                    break;
            }

            return Ok();
        }
        catch (StripeException ex)
        {
            _logger.LogWarning(ex, "Stripe webhook signature verification failed");
            return BadRequest();
        }
    }
}
