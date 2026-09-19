using MKPOS.Application.Abstractions;
using MKPOS.Application.Abstractions.Repositories;
using MKPOS.Application.DTOs;
using MKPOS.Domain.Entities;

namespace MKPOS.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly ICompanyRepository _companies;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(
        IUserRepository users,
        ICompanyRepository companies,
        IPasswordHasher passwordHasher)
    {
        _users = users;
        _companies = companies;
        _passwordHasher = passwordHasher;
    }

    public async Task<bool> NeedsInitialSetupAsync(CancellationToken ct = default)
    {
        return await _users.GetFirstAdminAsync(ct) is null;
    }

    public async Task<AuthResult> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return AuthResult.Fail("Ingrese usuario y contraseña.");

        var user = await _users.GetByUsernameAsync(username.Trim(), ct);

        if (user is null || !user.IsActive)
            return AuthResult.Fail("Usuario o contraseña incorrectos.");

        if (!_passwordHasher.Verify(password, user.PasswordHash, user.PasswordSalt))
            return AuthResult.Fail("Usuario o contraseña incorrectos.");

        return AuthResult.Ok(user);
    }

    public async Task<AuthResult> CreateFirstAdminAsync(SetupRequest request, CancellationToken ct = default)
    {
        if (await _users.GetFirstAdminAsync(ct) is not null)
            return AuthResult.Fail("La configuración inicial ya fue realizada.");

        if (string.IsNullOrWhiteSpace(request.CompanyName))
            return AuthResult.Fail("El nombre de la empresa es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.AdminUsername) || string.IsNullOrWhiteSpace(request.AdminPassword))
            return AuthResult.Fail("El usuario y la contraseña del administrador son obligatorios.");

        if (await _users.ExistsAsync(request.AdminUsername.Trim(), ct))
            return AuthResult.Fail("El nombre de usuario ya existe.");

        var (hash, salt) = _passwordHasher.Hash(request.AdminPassword);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.AdminUsername.Trim(),
            DisplayName = string.IsNullOrWhiteSpace(request.AdminDisplayName)
                ? request.AdminUsername.Trim()
                : request.AdminDisplayName.Trim(),
            PasswordHash = hash,
            PasswordSalt = salt,
            IsActive = true,
            IsAdmin = true,
            CreatedAt = DateTime.UtcNow
        };

        await _users.AddAsync(user, ct);

        var company = new Company
        {
            Id = Guid.NewGuid(),
            Name = request.CompanyName.Trim(),
            TaxId = request.TaxId,
            Address = request.Address,
            Phone = request.Phone,
            Email = request.Email,
            CurrencySymbol = string.IsNullOrWhiteSpace(request.CurrencySymbol) ? "$" : request.CurrencySymbol.Trim(),
            TaxRate = request.TaxRate,
            ReceiptFooter = request.ReceiptFooter,
            CreatedAt = DateTime.UtcNow
        };

        await _companies.SaveAsync(company, ct);

        return AuthResult.Ok(user);
    }
}