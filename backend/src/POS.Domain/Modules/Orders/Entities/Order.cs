using POS.Domain.Common;
using POS.Domain.Modules.Orders.Enums;

namespace POS.Domain.Modules.Orders.Entities;

public class Order : BaseEntity
{
    public Guid LocationId { get; set; }
    public Guid CashierUserId { get; set; }
    public Guid? CustomerId { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Open;

    public decimal Subtotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal TipTotal { get; set; }
    public decimal GrandTotal { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
