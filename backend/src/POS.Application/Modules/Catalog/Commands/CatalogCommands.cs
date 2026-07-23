using MediatR;
using POS.Application.Modules.Catalog.DTOs;
using POS.Application.Modules.Catalog.Services;

namespace POS.Application.Modules.Catalog.Commands;

public record CreateProductCommand(Guid TenantId, CreateProductRequest Request) : IRequest<ProductDto>;
public record UpdateProductCommand(Guid TenantId, Guid ProductId, UpsertProductRequest Request, Guid LocationId) : IRequest<ProductDto?>;
public record DeleteProductCommand(Guid TenantId, Guid ProductId) : IRequest<bool>;
public record AdjustStockCommand(Guid TenantId, Guid ProductId, Guid LocationId, int NewQuantity) : IRequest<bool>;
public record CreateCategoryCommand(Guid TenantId, UpsertCategoryRequest Request) : IRequest<CategoryDto>;
public record UpdateCategoryCommand(Guid TenantId, Guid CategoryId, UpsertCategoryRequest Request) : IRequest<CategoryDto?>;
public record DeleteCategoryCommand(Guid TenantId, Guid CategoryId) : IRequest<bool>;
public record AdjustInventoryCommand(Guid TenantId, Guid InventoryItemId, AdjustInventoryRequest Request) : IRequest<bool>;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly ProductService _productService;
    public CreateProductCommandHandler(ProductService productService) => _productService = productService;
    public Task<ProductDto> Handle(CreateProductCommand request, CancellationToken ct) =>
        _productService.CreateAsync(request.TenantId, request.Request, ct);
}

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductDto?>
{
    private readonly ProductService _productService;
    public UpdateProductCommandHandler(ProductService productService) => _productService = productService;
    public Task<ProductDto?> Handle(UpdateProductCommand request, CancellationToken ct) =>
        _productService.UpdateAsync(request.TenantId, request.ProductId, request.Request, request.LocationId, ct);
}

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly ProductService _productService;
    public DeleteProductCommandHandler(ProductService productService) => _productService = productService;
    public Task<bool> Handle(DeleteProductCommand request, CancellationToken ct) =>
        _productService.DeleteAsync(request.TenantId, request.ProductId, ct);
}

public class AdjustStockCommandHandler : IRequestHandler<AdjustStockCommand, bool>
{
    private readonly ProductService _productService;
    public AdjustStockCommandHandler(ProductService productService) => _productService = productService;
    public Task<bool> Handle(AdjustStockCommand request, CancellationToken ct) =>
        _productService.AdjustStockAsync(request.TenantId, request.ProductId, request.LocationId, request.NewQuantity, ct);
}

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    private readonly CategoryService _categoryService;
    public CreateCategoryCommandHandler(CategoryService categoryService) => _categoryService = categoryService;
    public Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken ct) =>
        _categoryService.CreateAsync(request.TenantId, request.Request, ct);
}

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, CategoryDto?>
{
    private readonly CategoryService _categoryService;
    public UpdateCategoryCommandHandler(CategoryService categoryService) => _categoryService = categoryService;
    public Task<CategoryDto?> Handle(UpdateCategoryCommand request, CancellationToken ct) =>
        _categoryService.UpdateAsync(request.TenantId, request.CategoryId, request.Request, ct);
}

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, bool>
{
    private readonly CategoryService _categoryService;
    public DeleteCategoryCommandHandler(CategoryService categoryService) => _categoryService = categoryService;
    public Task<bool> Handle(DeleteCategoryCommand request, CancellationToken ct) =>
        _categoryService.DeleteAsync(request.TenantId, request.CategoryId, ct);
}

public class AdjustInventoryCommandHandler : IRequestHandler<AdjustInventoryCommand, bool>
{
    private readonly InventoryService _inventoryService;
    public AdjustInventoryCommandHandler(InventoryService inventoryService) => _inventoryService = inventoryService;
    public Task<bool> Handle(AdjustInventoryCommand request, CancellationToken ct) =>
        _inventoryService.AdjustAsync(request.TenantId, request.InventoryItemId, request.Request, ct);
}
