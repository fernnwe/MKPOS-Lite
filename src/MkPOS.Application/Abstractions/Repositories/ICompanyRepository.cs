using MKPOS.Domain.Entities;

namespace MKPOS.Application.Abstractions.Repositories;

public interface ICompanyRepository
{
    Task<Company?> GetCurrentAsync(CancellationToken ct = default);
    Task SaveAsync(Company company, CancellationToken ct = default);
}