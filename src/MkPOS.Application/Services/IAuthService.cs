using MKPOS.Application.DTOs;

namespace MKPOS.Application.Services;

public interface IAuthService
{
    Task<bool> NeedsInitialSetupAsync(CancellationToken ct = default);
    Task<AuthResult> LoginAsync(string username, string password, CancellationToken ct = default);
    Task<AuthResult> CreateFirstAdminAsync(SetupRequest request, CancellationToken ct = default);
}