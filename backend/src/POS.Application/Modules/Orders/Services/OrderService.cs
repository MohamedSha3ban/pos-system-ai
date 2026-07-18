using Microsoft.EntityFrameworkCore;
using POS.Application.Common.Interfaces;
using POS.Application.Modules.Orders.DTOs;
using POS.Application.Modules.Orders.Interfaces;
using POS.Domain.Modules.Orders.Entities;
using POS.Domain.Modules.Orders.Enums;

namespace POS.Application.Modules.Orders.Services;

/// <summary>
/// Core checkout flow, shared by both the staff-facing in-store POS and the
/// customer-facing storefront: builds the order, decrements stock, and charges each
/// tender through the matching IPaymentProcessor. Supports split tenders.
/// </summary>
public class OrderService
{
    private readonly IApplicationDbContext _db;
    private readonly IEnumerable<IPaymentProcessor> _paymentProcessors;

    public OrderService(IApplicationDbContext db, IEnumerable<IPaymentProcessor> paymentProcessors)
    {
        _db = db;
        _paymentProcessors = paymentProcessors;
    }

    /// <param name="cashierUserId">Set for in-store staff checkout; null for online customer self-checkout.</param>
    /// <param name="customerId">Explicit actor for online checkout; for in-store, falls back to request.CustomerId (a cashier attaching a loyalty account) when not supplied.</param>
    public async Task<OrderResponse> CreateOrderAsync(
        Guid tenantId, Guid? cashierUserId, Guid? customerId, OrderChannel channel,
        CreateOrderRequest request, CancellationToken ct = default)
    {
        var productIds = request.Items.Select(i => i.ProductId).ToList();
        var products = await _db.Products
            .Where(p => productIds.Contains(p.Id) && p.TenantId == tenantId)
            .ToDictionaryAsync(p => p.Id, ct);

        var order = new Order
        {
            TenantId = tenantId,
            LocationId = request.LocationId,
            Channel = channel,
            CashierUserId = cashierUserId,
            CustomerId = customerId ?? request.CustomerId,
            TipTotal = request.TipTotal
        };

        decimal subtotal = 0, discountTotal = 0;

        foreach (var item in request.Items)
        {
            if (!products.TryGetValue(item.ProductId, out var product))
                throw new InvalidOperationException($"Product {item.ProductId} not found.");

            var lineDiscount = item.LineDiscount ?? 0;
            var lineTotal = (product.Price * item.Quantity) - lineDiscount;

            order.Items.Add(new OrderItem
            {
                TenantId = tenantId,
                ProductId = product.Id,
                ProductNameSnapshot = product.Name,
                UnitPrice = product.Price,
                Quantity = item.Quantity,
                LineDiscount = lineDiscount,
                LineTotal = lineTotal
            });

            subtotal += product.Price * item.Quantity;
            discountTotal += lineDiscount;

            var inventory = await _db.InventoryItems.FirstOrDefaultAsync(
                i => i.ProductId == product.Id && i.LocationId == request.LocationId, ct);
            if (inventory != null)
                inventory.QuantityOnHand -= item.Quantity;
        }

        // NOTE: plug in a real tax engine here (region-specific rules per the plan).
        var taxTotal = Math.Round((subtotal - discountTotal) * 0.0m, 2);
        order.Subtotal = subtotal;
        order.DiscountTotal = discountTotal;
        order.TaxTotal = taxTotal;
        order.GrandTotal = subtotal - discountTotal + taxTotal + request.TipTotal;

        var tenderTotal = request.Tenders.Sum(t => t.Amount);
        if (tenderTotal != order.GrandTotal)
            throw new InvalidOperationException("Tender total does not match order grand total.");

        foreach (var tender in request.Tenders)
        {
            var processor = _paymentProcessors.FirstOrDefault(p => p.SupportedMethod == tender.Method)
                ?? throw new InvalidOperationException($"No payment processor registered for {tender.Method}.");

            var result = await processor.ChargeAsync(tenantId, tender.Amount, "usd", tender.PaymentToken, ct);

            order.Payments.Add(new Payment
            {
                TenantId = tenantId,
                Method = tender.Method,
                Amount = tender.Amount,
                Status = result.Success ? PaymentStatus.Captured : PaymentStatus.Failed,
                ProcessorReference = result.ProcessorReference,
                ProcessorName = processor.GetType().Name
            });

            if (!result.Success)
                throw new InvalidOperationException($"Payment failed: {result.FailureReason}");
        }

        order.Status = OrderStatus.Completed;
        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);

        return new OrderResponse(order.Id, order.Status, order.Channel, order.Subtotal, order.TaxTotal, order.DiscountTotal, order.TipTotal, order.GrandTotal, order.CreatedAtUtc);
    }
}
