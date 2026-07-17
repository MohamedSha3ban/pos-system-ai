using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using POS.Infrastructure.Modules.Payments;

namespace POS.API.Controllers.Payments;

/// <summary>
/// Receives async payment status updates from Stripe (e.g. a card that required 3DS
/// confirmation, or a delayed payment method settling). Register this URL in the Stripe
/// dashboard as https://your-api-host/api/payments/stripe/webhook.
/// This endpoint is intentionally NOT [Authorize] -- Stripe signs the payload instead;
/// we verify that signature below rather than requiring a bearer token.
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
                    // TODO: look up the Payment row by ProcessorReference == succeeded.Id
                    // and mark it Captured if it isn't already (covers delayed/async methods).
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
