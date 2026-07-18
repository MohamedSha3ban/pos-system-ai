using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Modules.Orders.DTOs;
using POS.Application.Modules.Orders.Services;
using POS.Domain.Modules.Orders.Enums;

namespace POS.API.Controllers.Storefront;

/// <summary>Customer self-checkout -- requires a customer JWT from StorefrontAuthController.</summary>
[ApiController]
[Authorize]
[Route("api/storefront/checkout")]
public class StorefrontOrdersController : ControllerBase
{
    private readonly OrderService _orderService;
    public StorefrontOrdersController(OrderService orderService) => _orderService = orderService;

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenantId")!);
    private Guid CustomerId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private bool IsCustomer => User.FindFirstValue("actorType") == "customer";

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> Checkout(CreateOrderRequest request, CancellationToken ct)
    {
        if (!IsCustomer) return Forbid(); // staff tokens shouldn't be usable here -- keep the two checkout paths distinct
        return Ok(await _orderService.CreateOrderAsync(TenantId, cashierUserId: null, CustomerId, OrderChannel.Online, request, ct));
    }
}
