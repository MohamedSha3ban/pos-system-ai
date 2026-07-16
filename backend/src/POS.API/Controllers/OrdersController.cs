using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.DTOs.Orders;
using POS.Application.Services;

namespace POS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;
    public OrdersController(OrderService orderService) => _orderService = orderService;

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenantId")!);
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    /// <summary>Checkout endpoint: creates the order, decrements stock, charges tenders.</summary>
    [HttpPost("checkout")]
    public async Task<ActionResult<OrderResponse>> Checkout(CreateOrderRequest request, CancellationToken ct)
        => Ok(await _orderService.CreateOrderAsync(TenantId, UserId, request, ct));
}
