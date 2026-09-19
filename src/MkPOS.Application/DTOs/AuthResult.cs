using MKPOS.Domain.Entities;

namespace MKPOS.Application.DTOs;

/// <summary>
/// Resultado de una operación de autenticación o configuración inicial.
/// </summary>
public sealed class AuthResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public User? User { get; init; }
    public bool NeedsSetup { get; init; }

    public static AuthResult Ok(User user) => new() { Success = true, User = user };
    public static AuthResult SetupRequired => new() { NeedsSetup = true };
    public static AuthResult Fail(string error) => new() { Error = error };
}