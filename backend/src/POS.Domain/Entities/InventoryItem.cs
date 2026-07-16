using POS.Domain.Common;

namespace POS.Domain.Entities;

/// <summary>
/// Stock level of a Product at a specific Location.
/// ReorderPoint/ReorderQuantity are seeded by the AI forecasting service
/// (see POS.Application/Services/ForecastingService) but editable by staff.
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
