using MediatR;
using POS.Application.Modules.Catalog.DTOs;
using POS.Application.Modules.Catalog.Services;

namespace POS.Application.Modules.Catalog.Queries;

public record GetCatalogQuery(Guid TenantId, Guid LocationId) : IRequest<List<ProductDto>>;
public record GetProductByIdQuery(Guid TenantId, Guid ProductId, Guid LocationId) : IRequest<ProductDto?>;
public record GetCategoriesQuery(Guid TenantId) : IRequest<List<CategoryDto>>;
public record GetInventoryQuery(Guid TenantId, Guid? LocationId) : IRequest<List<InventoryItemDto>>;

public class GetCatalogQueryHandler : IRequestHandler<GetCatalogQuery, List<ProductDto>>
{
    private readonly ProductService _productService;
    public GetCatalogQueryHandler(ProductService productService) => _productService = productService;
    public Task<List<ProductDto>> Handle(GetCatalogQuery request, CancellationToken ct) =>
        _productService.GetCatalogAsync(request.TenantId, request.LocationId, ct);
}

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly ProductService _productService;
    public GetProductByIdQueryHandler(ProductService productService) => _productService = productService;
    public Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken ct) =>
        _productService.GetByIdAsync(request.TenantId, request.ProductId, request.LocationId, ct);
}

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, List<CategoryDto>>
{
    private readonly CategoryService _categoryService;
    public GetCategoriesQueryHandler(CategoryService categoryService) => _categoryService = categoryService;
    public Task<List<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken ct) =>
        _categoryService.GetAllAsync(request.TenantId, ct);
}

public class GetInventoryQueryHandler : IRequestHandler<GetInventoryQuery, List<InventoryItemDto>>
{
    private readonly InventoryService _inventoryService;
    public GetInventoryQueryHandler(InventoryService inventoryService) => _inventoryService = inventoryService;
    public Task<List<InventoryItemDto>> Handle(GetInventoryQuery request, CancellationToken ct) =>
        _inventoryService.GetAllAsync(request.TenantId, request.LocationId, ct);
}
