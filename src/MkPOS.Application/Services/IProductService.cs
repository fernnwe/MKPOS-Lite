using MKPOS.Application.DTOs;

namespace MKPOS.Application.Services;

public interface IProductService
{
    Task<IReadOnlyList<ProductDto>> GetProductsAsync(
        string? search,
        Guid? categoryId,
        bool includeInactive = false,
        CancellationToken ct = default);

    Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<OperationResult> CreateAsync(ProductInput input, CancellationToken ct = default);
    Task<OperationResult> UpdateAsync(Guid id, ProductInput input, CancellationToken ct = default);

    /// <summary>Ajusta la existencia a un valor absoluto (conteo físico).</summary>
    Task<OperationResult> AdjustStockAsync(Guid id, decimal newStock, CancellationToken ct = default);

    Task<OperationResult> ToggleActiveAsync(Guid id, CancellationToken ct = default);
}