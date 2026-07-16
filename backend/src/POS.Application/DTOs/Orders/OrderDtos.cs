using POS.Domain.Enums;

namespace POS.Application.DTOs.Orders;

public record CreateOrderItemRequest(Guid ProductId, int Quantity, decimal? LineDiscount);

public record CreateOrderTenderRequest(PaymentMethod Method, decimal Amount);

public record CreateOrderRequest(
    Guid LocationId,
    Guid? CustomerId,
    List<CreateOrderItemRequest> Items,
    List<CreateOrderTenderRequest> Tenders,
    decimal TipTotal);

public record OrderResponse(
    Guid Id,
    OrderStatus Status,
    decimal Subtotal,
    decimal TaxTotal,
    decimal DiscountTotal,
    decimal TipTotal,
    decimal GrandTotal,
    DateTime CreatedAtUtc);
