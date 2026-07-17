namespace POS.Application.Modules.Insights.Interfaces;

public record DemandForecast(Guid ProductId, string ProductName, int PredictedUnitsNext7Days, int SuggestedReorderQuantity);

/// <summary>
/// AI layer entry point for inventory forecasting. The starter implementation (in the
/// Infrastructure Insights module) uses a moving-average baseline; swap in a real model
/// or an external ML service later without changing callers.
/// </summary>
public interface IForecastingService
{
    Task<List<DemandForecast>> GetLowStockForecastsAsync(Guid tenantId, Guid locationId, CancellationToken ct = default);
}
