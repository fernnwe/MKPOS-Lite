using Microsoft.EntityFrameworkCore;
using MKPOS.Application.Abstractions.Repositories;
using MKPOS.Domain.Entities;

namespace MKPOS.Infrastructure.Persistence.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly MKPOSDbContext _db;

    public ProductRepository(MKPOSDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(
        string? search,
        Guid? categoryId,
        bool includeInactive,
        CancellationToken ct = default)
    {
        var query = _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(p => p.IsActive);
        }

        if (categoryId is { } id)
        {
            query = query.Where(p => p.CategoryId == id);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p =>
                p.Name.Contains(term)
                || p.Sku.Contains(term)
                || (p.Barcode != null && p.Barcode.Contains(term)));
        }

        return await query
            .OrderBy(p => p.Name)
            .ToListAsync(ct);
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<bool> ExistsSkuAsync(string sku, Guid? excludeId, CancellationToken ct = default)
    {
        return await _db.Products
            .AsNoTracking()
            .AnyAsync(p => EF.Functions.Collate(p.Sku, "NOCASE") == sku && p.Id != excludeId, ct);
    }

    public async Task<bool> ExistsBarcodeAsync(string barcode, Guid? excludeId, CancellationToken ct = default)
    {
        return await _db.Products
            .AsNoTracking()
            .AnyAsync(p => p.Barcode != null
                && EF.Functions.Collate(p.Barcode, "NOCASE") == barcode
                && p.Id != excludeId, ct);
    }

    public async Task AddAsync(Product product, CancellationToken ct = default)
    {
        await _db.Products.AddAsync(product, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        _db.Products.Update(product);
        await _db.SaveChangesAsync(ct);
    }
}