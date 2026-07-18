using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.API.Authorization;
using POS.Application.Modules.Orders.DTOs;
using POS.Application.Modules.Orders.Services;
using POS.Domain.Common;
using POS.Domain.Modules.Orders.Enums;

namespace POS.API.Controllers.Orders;

/// <summary>Staff-facing, in-store checkout. For customer self-checkout, see StorefrontOrdersController.</summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;
    public OrdersController(OrderService orderService) => _orderService = orderService;

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenantId")!);
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    [HttpPost("checkout")]
    [RequirePermission(Permissions.OrdersCheckout)]
    public async Task<ActionResult<OrderResponse>> Checkout(CreateOrderRequest request, CancellationToken ct)
        => Ok(await _orderService.CreateOrderAsync(TenantId, UserId, customerId: null, OrderChannel.InStore, request, ct));
}
