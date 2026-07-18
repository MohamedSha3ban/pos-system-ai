using POS.Domain.Modules.Orders.Enums;

namespace POS.Application.Modules.Orders.DTOs;

public record CreateOrderItemRequest(Guid ProductId, int Quantity, decimal? LineDiscount);

public record CreateOrderTenderRequest(PaymentMethod Method, decimal Amount, string? PaymentToken);

public record CreateOrderRequest(
    Guid LocationId,
    Guid? CustomerId,
    List<CreateOrderItemRequest> Items,
    List<CreateOrderTenderRequest> Tenders,
    decimal TipTotal);

public record OrderResponse(
    Guid Id,
    OrderStatus Status,
    OrderChannel Channel,
    decimal Subtotal,
    decimal TaxTotal,
    decimal DiscountTotal,
    decimal TipTotal,
    decimal GrandTotal,
    DateTime CreatedAtUtc);
