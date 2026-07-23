using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Modules.Orders.Commands;
using POS.Application.Modules.Orders.DTOs;
using POS.Domain.Modules.Orders.Enums;

namespace POS.Gateway.Ecommerce.Controllers.Storefront;

/// <summary>Customer self-checkout -- requires a customer JWT from StorefrontAuthController.</summary>
[ApiController]
[Authorize]
[Route("api/storefront/checkout")]
public class StorefrontOrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    public StorefrontOrdersController(IMediator mediator) => _mediator = mediator;

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenantId")!);
    private Guid CustomerId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private bool IsCustomer => User.FindFirstValue("actorType") == "customer";

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> Checkout(CreateOrderRequest request, CancellationToken ct)
    {
        if (!IsCustomer) return Forbid(); // staff tokens are never issued by this gateway anyway, but keep the guard explicit
        return Ok(await _mediator.Send(new CheckoutCommand(TenantId, null, CustomerId, OrderChannel.Online, request), ct));
    }
}
