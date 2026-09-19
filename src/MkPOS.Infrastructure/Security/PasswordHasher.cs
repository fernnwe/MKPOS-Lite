using System.Security.Cryptography;
using MKPOS.Application.Abstractions;

namespace MKPOS.Infrastructure.Security;

/// <summary>
/// Hashes contraseñas usando PBKDF2 (Rfc2898DeriveBytes) con SHA-256
/// y un número elevado de iteraciones.
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 32;
    private const int HashSize = 32;
    private const int Iterations = 100_000;

    public (string Hash, string Salt) Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    public bool Verify(string password, string hash, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        var expected = Convert.FromBase64String(hash);

        var actual = Rfc2898DeriveBytes.Pbkdf2(
            password,
            saltBytes,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}