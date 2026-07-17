using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Modules.Catalog.DTOs;
using POS.Application.Modules.Catalog.Services;

namespace POS.API.Controllers.Catalog;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _productService;
    public ProductsController(ProductService productService) => _productService = productService;

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenantId")!);

    [HttpGet]
    public async Task<ActionResult<List<ProductDto>>> GetCatalog([FromQuery] Guid locationId, CancellationToken ct)
        => Ok(await _productService.GetCatalogAsync(TenantId, locationId, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, [FromQuery] Guid locationId, CancellationToken ct)
    {
        var product = await _productService.GetByIdAsync(TenantId, id, locationId, ct);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(CreateProductRequest request, CancellationToken ct)
        => Ok(await _productService.CreateAsync(TenantId, request, ct));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductDto>> Update(Guid id, [FromQuery] Guid locationId, UpsertProductRequest request, CancellationToken ct)
    {
        var updated = await _productService.UpdateAsync(TenantId, id, request, locationId, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await _productService.DeleteAsync(TenantId, id, ct);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPatch("{id:guid}/stock")]
    public async Task<IActionResult> AdjustStock(Guid id, [FromQuery] Guid locationId, [FromQuery] int quantity, CancellationToken ct)
    {
        var updated = await _productService.AdjustStockAsync(TenantId, id, locationId, quantity, ct);
        return updated ? NoContent() : NotFound();
    }
}
