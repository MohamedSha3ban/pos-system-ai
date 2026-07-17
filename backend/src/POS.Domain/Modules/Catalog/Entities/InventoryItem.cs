using POS.Domain.Common;

namespace POS.Domain.Modules.Catalog.Entities;

/// <summary>
/// Stock level of a Product at a Location. ReorderPoint/ReorderQuantity are seeded by
/// the Insights module's forecasting service but editable by staff.
/// </summary>
public class InventoryItem : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public Guid LocationId { get; set; }
    public int QuantityOnHand { get; set; }
    public int ReorderPoint { get; set; } = 5;
    public int ReorderQuantity { get; set; } = 20;
}
