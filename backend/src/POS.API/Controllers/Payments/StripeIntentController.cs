using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace POS.API.Controllers.Payments;

public record CreateIntentRequest(decimal Amount, string Currency = "usd");
public record CreateIntentResponse(string ClientSecret, string PaymentIntentId);

/// <summary>
/// Lets the web/mobile client collect card details via Stripe Elements/Stripe Terminal
/// *before* calling /api/orders/checkout. The client confirms the PaymentIntent (or Stripe
/// Terminal reader confirms it), then passes the resulting PaymentIntent/PaymentMethod id
/// as the tender's paymentToken on checkout.
/// </summary>
[ApiController]
[Authorize]
[Route("api/payments/stripe")]
public class StripeIntentController : ControllerBase
{
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenantId")!);

    [HttpPost("create-intent")]
    public async Task<ActionResult<CreateIntentResponse>> CreateIntent(CreateIntentRequest request, CancellationToken ct)
    {
        var service = new PaymentIntentService();
        var intent = await service.CreateAsync(new PaymentIntentCreateOptions
        {
            Amount = (long)Math.Round(request.Amount * 100, MidpointRounding.AwayFromZero),
            Currency = request.Currency,
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true },
            Metadata = new Dictionary<string, string> { { "tenantId", TenantId.ToString() } }
        }, cancellationToken: ct);

        return Ok(new CreateIntentResponse(intent.ClientSecret, intent.Id));
    }
}
