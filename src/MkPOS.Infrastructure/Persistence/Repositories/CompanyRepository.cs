using Microsoft.EntityFrameworkCore;
using MKPOS.Application.Abstractions.Repositories;
using MKPOS.Domain.Entities;

namespace MKPOS.Infrastructure.Persistence.Repositories;

public sealed class CompanyRepository : ICompanyRepository
{
    private readonly MKPOSDbContext _db;

    public CompanyRepository(MKPOSDbContext db)
    {
        _db = db;
    }

    public async Task<Company?> GetCurrentAsync(CancellationToken ct = default)
    {
        return await _db.Companies
            .AsNoTracking()
            .OrderBy(c => c.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task SaveAsync(Company company, CancellationToken ct = default)
    {
        var existing = await _db.Companies
            .OrderBy(c => c.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (existing is null)
        {
            await _db.Companies.AddAsync(company, ct);
        }
        else
        {
            existing.Name = company.Name;
            existing.TaxId = company.TaxId;
            existing.Address = company.Address;
            existing.Phone = company.Phone;
            existing.Email = company.Email;
            existing.CurrencySymbol = company.CurrencySymbol;
            existing.TaxRate = company.TaxRate;
            existing.ReceiptFooter = company.ReceiptFooter;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);
    }
}