namespace POS.Domain.Modules.Orders.Enums;

public enum PaymentStatus
{
    Pending,
    Authorized,
    Captured,
    Failed,
    Refunded,
    PartiallyRefunded
}
