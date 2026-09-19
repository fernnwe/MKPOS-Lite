using MKPOS.Infrastructure.Security;

namespace MKPOS.Tests;

public sealed class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void Hash_ProduceHashAndSalt_ForGivenPassword()
    {
        var (hash, salt) = _hasher.Hash("MiClaveSegura123!");

        Assert.False(string.IsNullOrWhiteSpace(hash));
        Assert.False(string.IsNullOrWhiteSpace(salt));
        Assert.Equal(44, hash.Length);
        Assert.Equal(44, salt.Length);
    }

    [Fact]
    public void Hash_GeneratesDifferentSalts_ForSamePassword()
    {
        var (_, salt1) = _hasher.Hash("misma-clave");
        var (_, salt2) = _hasher.Hash("misma-clave");

        Assert.NotEqual(salt1, salt2);
    }

    [Fact]
    public void Verify_ReturnsTrue_ForCorrectPassword()
    {
        var (hash, salt) = _hasher.Hash("clave-correcta");

        Assert.True(_hasher.Verify("clave-correcta", hash, salt));
    }

    [Fact]
    public void Verify_ReturnsFalse_ForIncorrectPassword()
    {
        var (hash, salt) = _hasher.Hash("clave-correcta");

        Assert.False(_hasher.Verify("clave-incorrecta", hash, salt));
    }
}