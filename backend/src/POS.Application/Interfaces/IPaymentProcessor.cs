using POS.Domain.Enums;

namespace POS.Application.Interfaces;

public record PaymentChargeResult(bool Success, string? ProcessorReference, string? FailureReason);

/// <summary>
/// The payment orchestration abstraction described in the plan: swap Stripe/Adyen/
/// a regional processor in and out without touching checkout logic.
/// Register the concrete implementation per PaymentMethod in DI (Infrastructure layer).
/// </summary>
public interface IPaymentProcessor
{
    PaymentMethod SupportedMethod { get; }
    Task<PaymentChargeResult> ChargeAsync(Guid tenantId, decimal amount, string currency, string? paymentToken, CancellationToken ct = default);
    Task<PaymentChargeResult> RefundAsync(string processorReference, decimal amount, CancellationToken ct = default);
}
