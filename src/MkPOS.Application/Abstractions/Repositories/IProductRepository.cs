using MKPOS.Domain.Entities;

namespace MKPOS.Application.Abstractions.Repositories;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetAllAsync(
        string? search,
        Guid? categoryId,
        bool includeInactive,
        CancellationToken ct = default);

    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsSkuAsync(string sku, Guid? excludeId, CancellationToken ct = default);
    Task<bool> ExistsBarcodeAsync(string barcode, Guid? excludeId, CancellationToken ct = default);
    Task AddAsync(Product product, CancellationToken ct = default);
    Task UpdateAsync(Product product, CancellationToken ct = default);
}