using MKPOS.Application.Abstractions.Repositories;
using MKPOS.Application.DTOs;
using MKPOS.Domain.Entities;

namespace MKPOS.Application.Services;

public sealed class ProductService : IProductService
{
    private readonly IProductRepository _products;
    private readonly ICategoryRepository _categories;

    public ProductService(IProductRepository products, ICategoryRepository categories)
    {
        _products = products;
        _categories = categories;
    }

    public async Task<IReadOnlyList<ProductDto>> GetProductsAsync(
        string? search,
        Guid? categoryId,
        bool includeInactive = false,
        CancellationToken ct = default)
    {
        var list = await _products.GetAllAsync(search, categoryId, includeInactive, ct);
        return list.Select(ToDto).ToList();
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _products.GetByIdAsync(id, ct);
        return product is null ? null : ToDto(product);
    }

    public async Task<OperationResult> CreateAsync(ProductInput input, CancellationToken ct = default)
    {
        var validation = await ValidateAsync(input, null, ct);
        if (validation is not null)
        {
            return validation;
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Sku = input.Sku.Trim(),
            Name = input.Name.Trim(),
            Description = Clean(input.Description),
            Barcode = Clean(input.Barcode),
            CategoryId = input.CategoryId,
            PurchasePrice = input.PurchasePrice,
            SalePrice = input.SalePrice,
            TaxRate = input.TaxRate,
            Stock = input.Stock,
            MinStock = input.MinStock,
            IsActive = input.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await _products.AddAsync(product, ct);
        return OperationResult.Ok();
    }

    public async Task<OperationResult> UpdateAsync(Guid id, ProductInput input, CancellationToken ct = default)
    {
        var product = await _products.GetByIdAsync(id, ct);
        if (product is null)
        {
            return OperationResult.Fail("El producto no existe.");
        }

        var validation = await ValidateAsync(input, id, ct);
        if (validation is not null)
        {
            return validation;
        }

        product.Sku = input.Sku.Trim();
        product.Name = input.Name.Trim();
        product.Description = Clean(input.Description);
        product.Barcode = Clean(input.Barcode);
        product.CategoryId = input.CategoryId;
        product.PurchasePrice = input.PurchasePrice;
        product.SalePrice = input.SalePrice;
        product.TaxRate = input.TaxRate;
        product.Stock = input.Stock;
        product.MinStock = input.MinStock;
        product.IsActive = input.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        await _products.UpdateAsync(product, ct);
        return OperationResult.Ok();
    }

    public async Task<OperationResult> AdjustStockAsync(Guid id, decimal newStock, CancellationToken ct = default)
    {
        if (newStock < 0)
        {
            return OperationResult.Fail("La existencia no puede ser negativa.");
        }

        var product = await _products.GetByIdAsync(id, ct);
        if (product is null)
        {
            return OperationResult.Fail("El producto no existe.");
        }

        product.Stock = newStock;
        product.UpdatedAt = DateTime.UtcNow;

        await _products.UpdateAsync(product, ct);
        return OperationResult.Ok();
    }

    public async Task<OperationResult> ToggleActiveAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _products.GetByIdAsync(id, ct);
        if (product is null)
        {
            return OperationResult.Fail("El producto no existe.");
        }

        product.IsActive = !product.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        await _products.UpdateAsync(product, ct);
        return OperationResult.Ok();
    }

    private async Task<OperationResult?> ValidateAsync(ProductInput input, Guid? excludeId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(input.Sku))
        {
            return OperationResult.Fail("El código (SKU) es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(input.Name))
        {
            return OperationResult.Fail("El nombre del producto es obligatorio.");
        }

        if (input.PurchasePrice < 0 || input.SalePrice < 0)
        {
            return OperationResult.Fail("Los precios no pueden ser negativos.");
        }

        if (input.TaxRate is < 0)
        {
            return OperationResult.Fail("El impuesto no puede ser negativo.");
        }

        if (input.Stock < 0 || input.MinStock < 0)
        {
            return OperationResult.Fail("Las existencias no pueden ser negativas.");
        }

        var sku = input.Sku.Trim();
        if (await _products.ExistsSkuAsync(sku, excludeId, ct))
        {
            return OperationResult.Fail("Ya existe un producto con ese código (SKU).");
        }

        var barcode = Clean(input.Barcode);
        if (barcode is not null && await _products.ExistsBarcodeAsync(barcode, excludeId, ct))
        {
            return OperationResult.Fail("Ya existe un producto con ese código de barras.");
        }

        if (input.CategoryId is { } categoryId
            && await _categories.GetByIdAsync(categoryId, ct) is null)
        {
            return OperationResult.Fail("La categoría seleccionada no existe.");
        }

        return null;
    }

    private static string? Clean(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static ProductDto ToDto(Product p)
    {
        return new ProductDto(
            p.Id,
            p.Sku,
            p.Name,
            p.Description,
            p.Barcode,
            p.CategoryId,
            p.Category?.Name,
            p.PurchasePrice,
            p.SalePrice,
            p.TaxRate,
            p.Stock,
            p.MinStock,
            p.IsActive);
    }
}