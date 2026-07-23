using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace POS.Gateway.Admin.Controllers.Payments;

public record CreateIntentRequest(decimal Amount, string Currency = "usd");
public record CreateIntentResponse(string ClientSecret, string PaymentIntentId);

/// <summary>
/// Lets staff collect card details via Stripe Terminal *before* calling
/// /api/orders/checkout -- e.g. tapping a card on an in-store reader. The resulting
/// PaymentIntent/PaymentMethod id is then passed as the tender's paymentToken on checkout.
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
