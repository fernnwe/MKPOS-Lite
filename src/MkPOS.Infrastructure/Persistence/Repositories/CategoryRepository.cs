using Microsoft.EntityFrameworkCore;
using MKPOS.Application.Abstractions.Repositories;
using MKPOS.Domain.Entities;

namespace MKPOS.Infrastructure.Persistence.Repositories;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly MKPOSDbContext _db;

    public CategoryRepository(MKPOSDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Category>> GetAllAsync(bool includeInactive, CancellationToken ct = default)
    {
        var query = _db.Categories.AsNoTracking();
        if (!includeInactive)
        {
            query = query.Where(c => c.IsActive);
        }

        return await query
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
    }

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<bool> ExistsNameAsync(string name, Guid? excludeId, CancellationToken ct = default)
    {
        return await _db.Categories
            .AsNoTracking()
            .AnyAsync(c => EF.Functions.Collate(c.Name, "NOCASE") == name && c.Id != excludeId, ct);
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetProductCountsAsync(CancellationToken ct = default)
    {
        return await _db.Products
            .AsNoTracking()
            .Where(p => p.CategoryId != null)
            .GroupBy(p => p.CategoryId!.Value)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Count, ct);
    }

    public async Task AddAsync(Category category, CancellationToken ct = default)
    {
        await _db.Categories.AddAsync(category, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Category category, CancellationToken ct = default)
    {
        _db.Categories.Update(category);
        await _db.SaveChangesAsync(ct);
    }
}