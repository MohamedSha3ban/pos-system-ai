using POS.Domain.Common;
using POS.Domain.Modules.Orders.Enums;

namespace POS.Domain.Modules.Orders.Entities;

/// <summary>
/// One tender on an order. An order can have multiple Payments (split tender).
/// </summary>
public class Payment : BaseEntity
{
    public Guid OrderId { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public decimal Amount { get; set; }
    public string? ProcessorReference { get; set; }
    public string? ProcessorName { get; set; }
}
