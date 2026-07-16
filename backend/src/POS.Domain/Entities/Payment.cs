using POS.Domain.Common;
using POS.Domain.Enums;

namespace POS.Domain.Entities;

/// <summary>
/// One tender on an order. An order can have multiple Payments (split tender:
/// e.g., part cash, part card).
/// </summary>
public class Payment : BaseEntity
{
    public Guid OrderId { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public decimal Amount { get; set; }
    public string? ProcessorReference { get; set; } // id from Stripe/Adyen/etc.
    public string? ProcessorName { get; set; }       // "Stripe", "Adyen", "Moyasar", ...
}
