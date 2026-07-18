namespace POS.Application.Modules.Catalog.DTOs;

public record InventoryItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string Sku,
    Guid LocationId,
    string LocationName,
    int QuantityOnHand,
    int ReorderPoint,
    int ReorderQuantity,
    bool IsLow);

public record AdjustInventoryRequest(int QuantityOnHand, int? ReorderPoint, int? ReorderQuantity);
