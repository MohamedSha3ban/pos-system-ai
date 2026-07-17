using POS.Domain.Common;

namespace POS.Domain.Modules.Catalog.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string Sku { get; set; } = default!;
    public string? Barcode { get; set; }
    public decimal Price { get; set; }
    public decimal? CostPrice { get; set; }
    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }
    public bool TrackInventory { get; set; } = true;
    public bool IsActive { get; set; } = true;

    public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
}
