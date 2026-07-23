using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Modules.Insights.Interfaces;
using POS.Application.Modules.Insights.Queries;

namespace POS.Gateway.Admin.Controllers.Insights;

/// <summary>AI-powered endpoints: low-stock/reorder forecasts.</summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class InsightsController : ControllerBase
{
    private readonly IMediator _mediator;
    public InsightsController(IMediator mediator) => _mediator = mediator;

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenantId")!);

    [HttpGet("reorder-suggestions")]
    public async Task<ActionResult<List<DemandForecast>>> ReorderSuggestions([FromQuery] Guid locationId, CancellationToken ct)
        => Ok(await _mediator.Send(new GetReorderSuggestionsQuery(TenantId, locationId), ct));
}
