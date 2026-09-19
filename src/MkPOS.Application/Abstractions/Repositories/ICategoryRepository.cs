using MKPOS.Domain.Entities;

namespace MKPOS.Application.Abstractions.Repositories;

public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> GetAllAsync(bool includeInactive, CancellationToken ct = default);
    Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsNameAsync(string name, Guid? excludeId, CancellationToken ct = default);
    Task<IReadOnlyDictionary<Guid, int>> GetProductCountsAsync(CancellationToken ct = default);
    Task AddAsync(Category category, CancellationToken ct = default);
    Task UpdateAsync(Category category, CancellationToken ct = default);
}