using POS.Application.Modules.Orders.Interfaces;
using POS.Domain.Modules.Orders.Enums;

namespace POS.Infrastructure.Modules.Payments;

/// <summary>Cash needs no external processor -- "succeeds" once the cashier confirms it in hand.</summary>
public class CashPaymentProcessor : IPaymentProcessor
{
    public PaymentMethod SupportedMethod => PaymentMethod.Cash;

    public Task<PaymentChargeResult> ChargeAsync(Guid tenantId, decimal amount, string currency, string? paymentToken, CancellationToken ct = default)
        => Task.FromResult(new PaymentChargeResult(true, $"CASH-{Guid.NewGuid():N}", null));

    public Task<PaymentChargeResult> RefundAsync(string processorReference, decimal amount, CancellationToken ct = default)
        => Task.FromResult(new PaymentChargeResult(true, processorReference, null));
}
