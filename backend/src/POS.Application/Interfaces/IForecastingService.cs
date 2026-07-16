namespace POS.Application.Interfaces;

public record DemandForecast(Guid ProductId, int PredictedUnitsNext7Days, int SuggestedReorderQuantity);

/// <summary>
/// AI layer entry point for inventory forecasting (Phase 2 of the plan).
/// The MVP implementation (POS.Infrastructure/Services/SimpleMovingAverageForecastingService)
/// uses a moving-average baseline; swap in a real model or call an ML service later
/// without changing callers.
/// </summary>
public interface IForecastingService
{
    Task<List<DemandForecast>> GetLowStockForecastsAsync(Guid tenantId, Guid locationId, CancellationToken ct = default);
}
