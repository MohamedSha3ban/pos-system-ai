using Microsoft.EntityFrameworkCore;
using POS.Application.Modules.Insights.Interfaces;
using POS.Infrastructure.Persistence;

namespace POS.Infrastructure.Modules.Insights;

/// <summary>
/// Baseline forecasting: 30-day moving average per product, projected 7 days forward,
/// flagged against ReorderPoint. Swap for a real time-series model (Prophet/LightGBM)
/// or an external ML service later -- callers only depend on IForecastingService.
/// </summary>
public class SimpleMovingAverageForecastingService : IForecastingService
{
    private readonly ApplicationDbContext _db;
    public SimpleMovingAverageForecastingService(ApplicationDbContext db) => _db = db;

    public async Task<List<DemandForecast>> GetLowStockForecastsAsync(Guid tenantId, Guid locationId, CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(-30);

        var salesByProduct = await _db.OrderItems
            .Where(oi => oi.TenantId == tenantId && oi.CreatedAtUtc >= cutoff)
            .GroupBy(oi => oi.ProductId)
            .Select(g => new { ProductId = g.Key, TotalUnits = g.Sum(x => x.Quantity) })
            .ToListAsync(ct);

        var inventory = await _db.InventoryItems
            .Include(i => i.Product)
            .Where(i => i.TenantId == tenantId && i.LocationId == locationId)
            .ToListAsync(ct);

        var forecasts = new List<DemandForecast>();
        foreach (var inv in inventory)
        {
            var sales = salesByProduct.FirstOrDefault(s => s.ProductId == inv.ProductId);
            var dailyAvg = (sales?.TotalUnits ?? 0) / 30.0;
            var predicted7Day = (int)Math.Ceiling(dailyAvg * 7);

            if (inv.QuantityOnHand <= inv.ReorderPoint || inv.QuantityOnHand < predicted7Day)
            {
                var suggested = Math.Max(inv.ReorderQuantity, predicted7Day * 2);
                forecasts.Add(new DemandForecast(inv.ProductId, inv.Product?.Name ?? "Unknown", predicted7Day, suggested));
            }
        }

        return forecasts;
    }
}
