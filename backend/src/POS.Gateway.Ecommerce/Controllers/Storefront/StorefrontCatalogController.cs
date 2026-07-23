using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Modules.Catalog.DTOs;
using POS.Application.Modules.Catalog.Queries;

namespace POS.Gateway.Ecommerce.Controllers.Storefront;

/// <summary>Public product browsing for a tenant's storefront -- no login required.</summary>
[ApiController]
[AllowAnonymous]
[Route("api/storefront/{tenantId:guid}/products")]
public class StorefrontCatalogController : ControllerBase
{
    private readonly IMediator _mediator;
    public StorefrontCatalogController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<List<ProductDto>>> GetCatalog(Guid tenantId, [FromQuery] Guid locationId, CancellationToken ct)
    {
        var catalog = await _mediator.Send(new GetCatalogQuery(tenantId, locationId), ct);
        return Ok(catalog.Where(p => p.IsActive));
    }
}
