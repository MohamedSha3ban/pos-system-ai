using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Interfaces;

namespace POS.API.Controllers;

/// <summary>AI-powered endpoints: low-stock/reorder forecasts (plan section 5.2).</summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class InsightsController : ControllerBase
{
    private readonly IForecastingService _forecastingService;
    public InsightsController(IForecastingService forecastingService) => _forecastingService = forecastingService;

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenantId")!);

    [HttpGet("reorder-suggestions")]
    public async Task<ActionResult<List<DemandForecast>>> ReorderSuggestions([FromQuery] Guid locationId, CancellationToken ct)
        => Ok(await _forecastingService.GetLowStockForecastsAsync(TenantId, locationId, ct));
}
