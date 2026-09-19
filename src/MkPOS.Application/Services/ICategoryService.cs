using MKPOS.Application.DTOs;

namespace MKPOS.Application.Services;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(bool includeInactive = false, CancellationToken ct = default);
    Task<OperationResult> CreateAsync(CategoryInput input, CancellationToken ct = default);
    Task<OperationResult> UpdateAsync(Guid id, CategoryInput input, CancellationToken ct = default);

    /// <summary>Borrado lógico: la categoría deja de estar activa pero conserva su historial.</summary>
    Task<OperationResult> DeleteAsync(Guid id, CancellationToken ct = default);
}