namespace POS.Application.DTOs.Products;

public record ProductDto(
    Guid Id,
    string Name,
    string Sku,
    string? Barcode,
    decimal Price,
    string? CategoryName,
    int QuantityOnHand);

public record CreateProductRequest(
    string Name,
    string Sku,
    string? Barcode,
    decimal Price,
    decimal? CostPrice,
    Guid? CategoryId,
    int InitialQuantity,
    Guid LocationId);
