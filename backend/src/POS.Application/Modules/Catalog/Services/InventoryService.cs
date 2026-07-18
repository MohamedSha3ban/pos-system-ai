using Microsoft.EntityFrameworkCore;
using POS.Application.Common.Interfaces;
using POS.Application.Modules.Catalog.DTOs;

namespace POS.Application.Modules.Catalog.Services;

/// <summary>Dedicated inventory view across all locations -- the admin portal's Inventory screen.</summary>
public class InventoryService
{
    private readonly IWriteDbContext _writeDb;
    private readonly IReadDbContext _readDb;

    public InventoryService(IWriteDbContext writeDb, IReadDbContext readDb)
    {
        _writeDb = writeDb;
        _readDb = readDb;
    }

    /// <summary>Independent list read -- read side.</summary>
    public async Task<List<InventoryItemDto>> GetAllAsync(Guid tenantId, Guid? locationId = null, CancellationToken ct = default)
    {
        var query = _readDb.InventoryItems
            .Include(i => i.Product)
            .Where(i => i.TenantId == tenantId && i.Product != null && !i.Product.IsDeleted);

        if (locationId.HasValue) query = query.Where(i => i.LocationId == locationId.Value);

        var locations = await _readDb.Locations.Where(l => l.TenantId == tenantId).ToListAsync(ct);

        var items = await query.ToListAsync(ct);
        return items.Select(i => new InventoryItemDto(
            i.Id, i.ProductId, i.Product!.Name, i.Product.Sku,
            i.LocationId, locations.FirstOrDefault(l => l.Id == i.LocationId)?.Name ?? "Unknown",
            i.QuantityOnHand, i.ReorderPoint, i.ReorderQuantity,
            i.QuantityOnHand <= i.ReorderPoint
        )).ToList();
    }

    public async Task<bool> AdjustAsync(Guid tenantId, Guid inventoryItemId, AdjustInventoryRequest request, CancellationToken ct = default)
    {
        var item = await _writeDb.InventoryItems.FirstOrDefaultAsync(i => i.TenantId == tenantId && i.Id == inventoryItemId, ct);
        if (item is null) return false;

        item.QuantityOnHand = request.QuantityOnHand;
        if (request.ReorderPoint.HasValue) item.ReorderPoint = request.ReorderPoint.Value;
        if (request.ReorderQuantity.HasValue) item.ReorderQuantity = request.ReorderQuantity.Value;
        item.UpdatedAtUtc = DateTime.UtcNow;

        await _writeDb.SaveChangesAsync(ct);
        return true;
    }
}
