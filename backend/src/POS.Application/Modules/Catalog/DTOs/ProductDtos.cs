namespace POS.Application.Modules.Catalog.DTOs;

public record ProductDto(
    Guid Id,
    string Name,
    string? Description,
    string Sku,
    string? Barcode,
    decimal Price,
    decimal? CostPrice,
    Guid? CategoryId,
    string? CategoryName,
    bool IsActive,
    int QuantityOnHand);

public record UpsertProductRequest(
    string Name,
    string? Description,
    string Sku,
    string? Barcode,
    decimal Price,
    decimal? CostPrice,
    Guid? CategoryId,
    bool IsActive);

/// <summary>Only used on create -- sets the starting stock count for a location.</summary>
public record CreateProductRequest(UpsertProductRequest Product, Guid LocationId, int InitialQuantity);
