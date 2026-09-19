using MKPOS.Application.Abstractions;
using MKPOS.Application.DTOs;
using MKPOS.Application.Services;
using MKPOS.Infrastructure.Security;
using MKPOS.Tests.Helpers;

namespace MKPOS.Tests;

public sealed class AuthServiceTests
{
    private static readonly SetupRequest ValidRequest = new(
        CompanyName: "Mi Tienda",
        TaxId: "XAXX010101000",
        Address: "Calle 1",
        Phone: "555-1234",
        Email: "tienda@example.com",
        CurrencySymbol: "$",
        TaxRate: 16m,
        ReceiptFooter: "Gracias por su compra",
        AdminUsername: "admin",
        AdminDisplayName: "Administrador",
        AdminPassword: "Admin.1234");

    private static AuthService CreateSut(
        out FakeUserRepository users,
        out FakeCompanyRepository companies)
    {
        users = new FakeUserRepository();
        companies = new FakeCompanyRepository();

        IPasswordHasher hasher = new PasswordHasher();

        return new AuthService(users, companies, hasher);
    }

    [Fact]
    public async Task NeedsInitialSetupAsync_ReturnsTrue_WhenNoAdminExists()
    {
        var sut = CreateSut(out _, out _);

        Assert.True(await sut.NeedsInitialSetupAsync());
    }

    [Fact]
    public async Task CreateFirstAdminAsync_CreatesUserAndCompany()
    {
        var sut = CreateSut(out var users, out var companies);

        var result = await sut.CreateFirstAdminAsync(ValidRequest);

        Assert.True(result.Success);
        Assert.NotNull(result.User);
        Assert.True(result.User!.IsAdmin);
        Assert.True(await sut.NeedsInitialSetupAsync() is false);

        var company = await companies.GetCurrentAsync();
        Assert.NotNull(company);
        Assert.Equal("Mi Tienda", company!.Name);
        Assert.Equal("$", company.CurrencySymbol);
    }

    [Fact]
    public async Task CreateFirstAdminAsync_Fails_WhenCompanyNameIsEmpty()
    {
        var sut = CreateSut(out _, out _);
        var request = ValidRequest with { CompanyName = "  " };

        var result = await sut.CreateFirstAdminAsync(request);

        Assert.False(result.Success);
        Assert.Equal("El nombre de la empresa es obligatorio.", result.Error);
    }

    [Fact]
    public async Task CreateFirstAdminAsync_Fails_WhenCalledTwice()
    {
        var sut = CreateSut(out _, out _);
        await sut.CreateFirstAdminAsync(ValidRequest);

        var result = await sut.CreateFirstAdminAsync(ValidRequest with { AdminUsername = "admin2" });

        Assert.False(result.Success);
        Assert.Equal("La configuración inicial ya fue realizada.", result.Error);
    }

    [Fact]
    public async Task LoginAsync_Succeeds_WithValidCredentials()
    {
        var sut = CreateSut(out _, out _);
        await sut.CreateFirstAdminAsync(ValidRequest);

        var result = await sut.LoginAsync("admin", "Admin.1234");

        Assert.True(result.Success);
        Assert.Equal("admin", result.User!.Username);
    }

    [Fact]
    public async Task LoginAsync_Fails_WithWrongPassword()
    {
        var sut = CreateSut(out _, out _);
        await sut.CreateFirstAdminAsync(ValidRequest);

        var result = await sut.LoginAsync("admin", "incorrecta");

        Assert.False(result.Success);
        Assert.Equal("Usuario o contraseña incorrectos.", result.Error);
    }

    [Fact]
    public async Task LoginAsync_Fails_ForUnknownUser()
    {
        var sut = CreateSut(out _, out _);

        var result = await sut.LoginAsync("nadie", "lo-sabe");

        Assert.False(result.Success);
        Assert.Equal("Usuario o contraseña incorrectos.", result.Error);
    }
}