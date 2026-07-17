using POS.Domain.Modules.Orders.Enums;

namespace POS.Application.Modules.Orders.Interfaces;

public record PaymentChargeResult(bool Success, string? ProcessorReference, string? FailureReason, string? ClientSecret = null);

/// <summary>
/// Payment orchestration abstraction (Payments module implements this). Swap processors
/// per PaymentMethod in DI without touching OrderService.
/// </summary>
public interface IPaymentProcessor
{
    PaymentMethod SupportedMethod { get; }
    Task<PaymentChargeResult> ChargeAsync(Guid tenantId, decimal amount, string currency, string? paymentToken, CancellationToken ct = default);
    Task<PaymentChargeResult> RefundAsync(string processorReference, decimal amount, CancellationToken ct = default);
}
