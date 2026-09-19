using MKPOS.Application.Abstractions.Repositories;
using MKPOS.Domain.Entities;

namespace MKPOS.Tests.Helpers;

public sealed class FakeUserRepository : IUserRepository
{
    private readonly List<User> _users = new();

    public Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        return Task.FromResult(_users.FirstOrDefault(u => u.Username == username));
    }

    public Task<User?> GetFirstAdminAsync(CancellationToken ct = default)
    {
        return Task.FromResult(_users.FirstOrDefault(u => u.IsAdmin));
    }

    public Task<bool> ExistsAsync(string username, CancellationToken ct = default)
    {
        return Task.FromResult(_users.Any(u => u.Username == username));
    }

    public Task AddAsync(User user, CancellationToken ct = default)
    {
        _users.Add(user);
        return Task.CompletedTask;
    }
}

public sealed class FakeCompanyRepository : ICompanyRepository
{
    private Company? _company;

    public Task<Company?> GetCurrentAsync(CancellationToken ct = default)
    {
        return Task.FromResult(_company);
    }

    public Task SaveAsync(Company company, CancellationToken ct = default)
    {
        _company = company;
        return Task.CompletedTask;
    }
}