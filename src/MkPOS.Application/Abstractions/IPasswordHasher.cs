namespace MKPOS.Application.Abstractions;

/// <summary>
/// Abstracción para el hashing y verificación de contraseñas.
/// </summary>
public interface IPasswordHasher
{
    (string Hash, string Salt) Hash(string password);
    bool Verify(string password, string hash, string salt);
}