using Microsoft.EntityFrameworkCore;
using POS.Application.Common.Interfaces;
using POS.Application.Modules.Catalog.DTOs;
using POS.Domain.Modules.Catalog.Entities;

namespace POS.Application.Modules.Catalog.Services;

public class ProductService
{
    private readonly IApplicationDbContext _db;
    public ProductService(IApplicationDbContext db) => _db = db;

    public async Task<List<ProductDto>> GetCatalogAsync(Guid tenantId, Guid locationId, CancellationToken ct = default)
    {
        return await _db.Products
            .Where(p => p.TenantId == tenantId && !p.IsDeleted)
            .Select(p => ToDto(p, locationId))
            .ToListAsync(ct);
    }

    public async Task<ProductDto?> GetByIdAsync(Guid tenantId, Guid productId, Guid locationId, CancellationToken ct = default)
    {
        var product = await _db.Products
            .Where(p => p.TenantId == tenantId && p.Id == productId && !p.IsDeleted)
            .Select(p => ToDto(p, locationId))
            .FirstOrDefaultAsync(ct);
        return product;
    }

    public async Task<ProductDto> CreateAsync(Guid tenantId, CreateProductRequest request, CancellationToken ct = default)
    {
        var product = new Product
        {
            TenantId = tenantId,
            Name = request.Product.Name,
            Description = request.Product.Description,
            Sku = request.Product.Sku,
            Barcode = request.Product.Barcode,
            Price = request.Product.Price,
            CostPrice = request.Product.CostPrice,
            CategoryId = request.Product.CategoryId,
            IsActive = request.Product.IsActive
        };
        _db.Products.Add(product);

        _db.InventoryItems.Add(new InventoryItem
        {
            TenantId = tenantId,
            ProductId = product.Id,
            LocationId = request.LocationId,
            QuantityOnHand = request.InitialQuantity
        });

        await _db.SaveChangesAsync(ct);
        return (await GetByIdAsync(tenantId, product.Id, request.LocationId, ct))!;
    }

    public async Task<ProductDto?> UpdateAsync(Guid tenantId, Guid productId, UpsertProductRequest request, Guid locationId, CancellationToken ct = default)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == productId && !p.IsDeleted, ct);
        if (product is null) return null;

        product.Name = request.Name;
        product.Description = request.Description;
        product.Sku = request.Sku;
        product.Barcode = request.Barcode;
        product.Price = request.Price;
        product.CostPrice = request.CostPrice;
        product.CategoryId = request.CategoryId;
        product.IsActive = request.IsActive;
        product.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(tenantId, productId, locationId, ct);
    }

    public async Task<bool> DeleteAsync(Guid tenantId, Guid productId, CancellationToken ct = default)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == productId && !p.IsDeleted, ct);
        if (product is null) return false;

        // Soft delete -- keeps historical OrderItem snapshots intact.
        product.IsDeleted = true;
        product.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> AdjustStockAsync(Guid tenantId, Guid productId, Guid locationId, int newQuantity, CancellationToken ct = default)
    {
        var inventory = await _db.InventoryItems.FirstOrDefaultAsync(
            i => i.TenantId == tenantId && i.ProductId == productId && i.LocationId == locationId, ct);
        if (inventory is null) return false;

        inventory.QuantityOnHand = newQuantity;
        inventory.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private static ProductDto ToDto(Product p, Guid locationId) => new(
        p.Id, p.Name, p.Description, p.Sku, p.Barcode, p.Price, p.CostPrice,
        p.CategoryId, p.Category != null ? p.Category.Name : null, p.IsActive,
        p.InventoryItems.Where(i => i.LocationId == locationId).Select(i => i.QuantityOnHand).FirstOrDefault());
}
