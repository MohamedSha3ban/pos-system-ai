using MediatR;
using POS.Application.Modules.Orders.DTOs;
using POS.Application.Modules.Orders.Services;
using POS.Domain.Modules.Orders.Enums;

namespace POS.Application.Modules.Orders.Commands;

/// <summary>
/// One command, two callers: the Admin/Mobile gateways' OrdersController sets
/// CashierUserId + Channel=InStore; the Ecommerce gateway's StorefrontOrdersController
/// sets CustomerId + Channel=Online. Same handler, same OrderService, same consistency
/// guarantees either way (see OrderService's doc comment on why it's write-context-only).
/// </summary>
public record CheckoutCommand(
    Guid TenantId, Guid? CashierUserId, Guid? CustomerId, OrderChannel Channel, CreateOrderRequest Request
) : IRequest<OrderResponse>;

public class CheckoutCommandHandler : IRequestHandler<CheckoutCommand, OrderResponse>
{
    private readonly OrderService _orderService;
    public CheckoutCommandHandler(OrderService orderService) => _orderService = orderService;
    public Task<OrderResponse> Handle(CheckoutCommand request, CancellationToken ct) =>
        _orderService.CreateOrderAsync(request.TenantId, request.CashierUserId, request.CustomerId, request.Channel, request.Request, ct);
}
