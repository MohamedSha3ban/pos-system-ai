using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.DTOs.Products;
using POS.Application.Services;

namespace POS.API.Controllers;

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

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(CreateProductRequest request, CancellationToken ct)
        => Ok(await _productService.CreateProductAsync(TenantId, request, ct));
}
