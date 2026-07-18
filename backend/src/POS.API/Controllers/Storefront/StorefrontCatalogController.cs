using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Modules.Catalog.DTOs;
using POS.Application.Modules.Catalog.Services;

namespace POS.API.Controllers.Storefront;

/// <summary>Public product browsing for a tenant's storefront -- no login required.</summary>
[ApiController]
[AllowAnonymous]
[Route("api/storefront/{tenantId:guid}/products")]
public class StorefrontCatalogController : ControllerBase
{
    private readonly ProductService _productService;
    public StorefrontCatalogController(ProductService productService) => _productService = productService;

    [HttpGet]
    public async Task<ActionResult<List<ProductDto>>> GetCatalog(Guid tenantId, [FromQuery] Guid locationId, CancellationToken ct)
        => Ok((await _productService.GetCatalogAsync(tenantId, locationId, ct)).Where(p => p.IsActive));
}
