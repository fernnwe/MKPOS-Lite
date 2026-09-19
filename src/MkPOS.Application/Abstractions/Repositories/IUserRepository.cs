using MKPOS.Domain.Entities;

namespace MKPOS.Application.Abstractions.Repositories;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<User?> GetFirstAdminAsync(CancellationToken ct = default);
    Task<bool> ExistsAsync(string username, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
}