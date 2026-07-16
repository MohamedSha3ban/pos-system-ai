using Microsoft.EntityFrameworkCore;
using POS.Application.DTOs.Products;
using POS.Application.Interfaces;
using POS.Domain.Entities;

namespace POS.Application.Services;

public class ProductService
{
    private readonly IApplicationDbContext _db;

    public ProductService(IApplicationDbContext db) => _db = db;

    public async Task<List<ProductDto>> GetCatalogAsync(Guid tenantId, Guid locationId, CancellationToken ct = default)
    {
        return await _db.Products
            .Where(p => p.TenantId == tenantId && !p.IsDeleted && p.IsActive)
            .Select(p => new ProductDto(
                p.Id,
                p.Name,
                p.Sku,
                p.Barcode,
                p.Price,
                p.Category != null ? p.Category.Name : null,
                p.InventoryItems.Where(i => i.LocationId == locationId).Select(i => i.QuantityOnHand).FirstOrDefault()
            ))
            .ToListAsync(ct);
    }

    public async Task<ProductDto> CreateProductAsync(Guid tenantId, CreateProductRequest request, CancellationToken ct = default)
    {
        var product = new Product
        {
            TenantId = tenantId,
            Name = request.Name,
            Sku = request.Sku,
            Barcode = request.Barcode,
            Price = request.Price,
            CostPrice = request.CostPrice,
            CategoryId = request.CategoryId
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

        return new ProductDto(product.Id, product.Name, product.Sku, product.Barcode, product.Price, null, request.InitialQuantity);
    }
}
