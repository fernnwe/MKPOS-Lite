using Microsoft.EntityFrameworkCore;
using MKPOS.Application.Abstractions.Repositories;
using MKPOS.Domain.Entities;

namespace MKPOS.Infrastructure.Persistence.Repositories;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly MKPOSDbContext _db;

    public CustomerRepository(MKPOSDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Customer>> GetAllAsync(string? search, bool includeInactive, CancellationToken ct = default)
    {
        var query = _db.Customers.AsNoTracking();
        if (!includeInactive)
        {
            query = query.Where(c => c.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c =>
                c.Name.Contains(term)
                || (c.Phone != null && c.Phone.Contains(term))
                || (c.Email != null && c.Email.Contains(term)));
        }

        return await query
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
    }

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task AddAsync(Customer customer, CancellationToken ct = default)
    {
        await _db.Customers.AddAsync(customer, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Customer customer, CancellationToken ct = default)
    {
        var tracked = await _db.Customers
            .FirstOrDefaultAsync(c => c.Id == customer.Id, ct);

        if (tracked is not null)
        {
            _db.Entry(tracked).CurrentValues.SetValues(customer);
        }
        else
        {
            _db.Customers.Update(customer);
        }

        await _db.SaveChangesAsync(ct);
    }
}