namespace MKPOS.Application.DTOs;

public sealed record ProductDto(
    Guid Id,
    string Sku,
    string Name,
    string? Description,
    string? Barcode,
    Guid? CategoryId,
    string? CategoryName,
    decimal PurchasePrice,
    decimal SalePrice,
    decimal? TaxRate,
    decimal Stock,
    decimal MinStock,
    bool IsActive);

public sealed record ProductInput(
    string Sku,
    string Name,
    string? Description,
    string? Barcode,
    Guid? CategoryId,
    decimal PurchasePrice,
    decimal SalePrice,
    decimal? TaxRate,
    decimal Stock,
    decimal MinStock,
    bool IsActive = true);