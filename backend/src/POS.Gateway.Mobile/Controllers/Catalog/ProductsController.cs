using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Modules.Catalog.Commands;
using POS.Application.Modules.Catalog.DTOs;
using POS.Application.Modules.Catalog.Queries;
using POS.Domain.Common;
using POS.Gateway.Mobile.Authorization;

namespace POS.Gateway.Mobile.Controllers.Catalog;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;
    public ProductsController(IMediator mediator) => _mediator = mediator;

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenantId")!);

    [HttpGet]
    public async Task<ActionResult<List<ProductDto>>> GetCatalog([FromQuery] Guid locationId, CancellationToken ct)
        => Ok(await _mediator.Send(new GetCatalogQuery(TenantId, locationId), ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, [FromQuery] Guid locationId, CancellationToken ct)
    {
        var product = await _mediator.Send(new GetProductByIdQuery(TenantId, id, locationId), ct);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    [RequirePermission(Permissions.ProductsManage)]
    public async Task<ActionResult<ProductDto>> Create(CreateProductRequest request, CancellationToken ct)
        => Ok(await _mediator.Send(new CreateProductCommand(TenantId, request), ct));

    [HttpPut("{id:guid}")]
    [RequirePermission(Permissions.ProductsManage)]
    public async Task<ActionResult<ProductDto>> Update(Guid id, [FromQuery] Guid locationId, UpsertProductRequest request, CancellationToken ct)
    {
        var updated = await _mediator.Send(new UpdateProductCommand(TenantId, id, request, locationId), ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.ProductsManage)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await _mediator.Send(new DeleteProductCommand(TenantId, id), ct);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPatch("{id:guid}/stock")]
    [RequirePermission(Permissions.InventoryManage)]
    public async Task<IActionResult> AdjustStock(Guid id, [FromQuery] Guid locationId, [FromQuery] int quantity, CancellationToken ct)
    {
        var updated = await _mediator.Send(new AdjustStockCommand(TenantId, id, locationId, quantity), ct);
        return updated ? NoContent() : NotFound();
    }
}
