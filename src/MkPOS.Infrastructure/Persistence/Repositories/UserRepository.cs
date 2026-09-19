using Microsoft.EntityFrameworkCore;
using MKPOS.Application.Abstractions.Repositories;
using MKPOS.Domain.Entities;

namespace MKPOS.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly MKPOSDbContext _db;

    public UserRepository(MKPOSDbContext db)
    {
        _db = db;
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        return await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username, ct);
    }

    public async Task<User?> GetFirstAdminAsync(CancellationToken ct = default)
    {
        return await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.IsAdmin, ct);
    }

    public async Task<bool> ExistsAsync(string username, CancellationToken ct = default)
    {
        return await _db.Users
            .AsNoTracking()
            .AnyAsync(u => u.Username == username, ct);
    }

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await _db.Users.AddAsync(user, ct);
        await _db.SaveChangesAsync(ct);
    }
}