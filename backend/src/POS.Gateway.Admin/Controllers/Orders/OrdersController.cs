using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Modules.Orders.Commands;
using POS.Application.Modules.Orders.DTOs;
using POS.Domain.Common;
using POS.Domain.Modules.Orders.Enums;
using POS.Gateway.Admin.Authorization;

namespace POS.Gateway.Admin.Controllers.Orders;

/// <summary>Staff-facing, in-store checkout. For customer self-checkout, see the Ecommerce gateway.</summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    public OrdersController(IMediator mediator) => _mediator = mediator;

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenantId")!);
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    [HttpPost("checkout")]
    [RequirePermission(Permissions.OrdersCheckout)]
    public async Task<ActionResult<OrderResponse>> Checkout(CreateOrderRequest request, CancellationToken ct)
        => Ok(await _mediator.Send(new CheckoutCommand(TenantId, UserId, null, OrderChannel.InStore, request), ct));
}
