using Microsoft.EntityFrameworkCore;
using MKPOS.Application.Abstractions.Repositories;
using MKPOS.Domain.Entities;

namespace MKPOS.Infrastructure.Persistence.Repositories;

public sealed class SaleRepository : ISaleRepository
{
    private readonly MKPOSDbContext _db;

    public SaleRepository(MKPOSDbContext db)
    {
        _db = db;
    }

    public async Task<int> GetLastTicketNumberAsync(CancellationToken ct = default)
    {
        return await _db.Sales
            .AsNoTracking()
            .OrderByDescending(s => s.TicketNumber)
            .Select(s => (int?)s.TicketNumber)
            .FirstOrDefaultAsync(ct) ?? 0;
    }

    public async Task CompleteAsync(Sale sale, IReadOnlyList<Product> productsToAdjust, Customer? customer, CancellationToken ct = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(ct);

        _db.Sales.Add(sale);

        foreach (var product in productsToAdjust)
        {
            var tracked = await _db.Products.FirstOrDefaultAsync(p => p.Id == product.Id, ct);
            if (tracked is null)
            {
                continue;
            }

            tracked.Stock = product.Stock;
            tracked.UpdatedAt = DateTime.UtcNow;
        }

        if (customer is not null)
        {
            var trackedCustomer = await _db.Customers.FirstOrDefaultAsync(c => c.Id == customer.Id, ct);
            if (trackedCustomer is not null)
            {
                trackedCustomer.Balance = customer.Balance;
                trackedCustomer.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }

    public async Task<IReadOnlyList<Sale>> GetRecentAsync(int take, CancellationToken ct = default)
    {
        return await _db.Sales
            .AsNoTracking()
            .Include(s => s.Items)
            .OrderByDescending(s => s.SaleDate)
            .Take(take)
            .ToListAsync(ct);
    }
}