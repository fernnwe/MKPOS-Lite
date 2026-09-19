using MKPOS.Application.DTOs;
using MKPOS.Application.Services;
using MKPOS.Tests.Helpers;

namespace MKPOS.Tests;

public sealed class CustomerServiceTests
{
    private const string ValidName = "Juan Pérez";

    private static CustomerService CreateSut(out FakeCustomerRepository customers)
    {
        customers = new FakeCustomerRepository();
        return new CustomerService(customers);
    }

    private static CustomerInput ValidInput(string? name = null)
    {
        return new CustomerInput(
            Name: name ?? ValidName,
            Phone: "55-1234-5678",
            Email: "juan@example.com",
            Address: "Av. Principal 123");
    }

    [Fact]
    public async Task CreateAsync_AddsCustomer()
    {
        var sut = CreateSut(out var customers);

        var result = await sut.CreateAsync(ValidInput());

        Assert.True(result.Success);
        var list = await customers.GetAllAsync(null, includeInactive: true);
        Assert.Single(list);
        Assert.Equal(ValidName, list[0].Name);
        Assert.Equal(0m, list[0].Balance);
    }

    [Fact]
    public async Task CreateAsync_Fails_OnBlankName()
    {
        var sut = CreateSut(out _);

        var result = await sut.CreateAsync(ValidInput(name: "  "));

        Assert.False(result.Success);
        Assert.Equal("El nombre del cliente es obligatorio.", result.Error);
    }

    [Fact]
    public async Task CreateAsync_Fails_OnInvalidEmail()
    {
        var sut = CreateSut(out _);

        var result = await sut.CreateAsync(ValidInput() with { Email = "no-es-email" });

        Assert.False(result.Success);
        Assert.Equal("El correo electrónico no tiene un formato válido.", result.Error);
    }

    [Fact]
    public async Task CreateAsync_AllowsNullOptionalFields()
    {
        var sut = CreateSut(out var customers);

        var result = await sut.CreateAsync(new CustomerInput(ValidName, null, null, "  "));

        Assert.True(result.Success);
        var list = await customers.GetAllAsync(null, includeInactive: true);
        var customer = Assert.Single(list);
        Assert.Null(customer.Phone);
        Assert.Null(customer.Email);
        Assert.Null(customer.Address);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesFields()
    {
        var sut = CreateSut(out var customers);
        await sut.CreateAsync(ValidInput());
        var created = (await customers.GetAllAsync(null, includeInactive: true))[0];

        var result = await sut.UpdateAsync(created.Id, ValidInput(name: "María López"));

        Assert.True(result.Success);
        var updated = (await customers.GetAllAsync(null, includeInactive: true))[0];
        Assert.Equal("María López", updated.Name);
    }

    [Fact]
    public async Task UpdateAsync_Fails_WhenCustomerMissing()
    {
        var sut = CreateSut(out _);

        var result = await sut.UpdateAsync(Guid.NewGuid(), ValidInput());

        Assert.False(result.Success);
        Assert.Equal("El cliente no existe.", result.Error);
    }

    [Fact]
    public async Task ApplyPaymentAsync_ReducesBalance()
    {
        var sut = CreateSut(out var customers);
        await sut.CreateAsync(ValidInput());
        var customer = (await customers.GetAllAsync(null, includeInactive: true))[0];
        customer.Balance = 200m;

        var result = await sut.ApplyPaymentAsync(customer.Id, 75m);

        Assert.True(result.Success);
        var updated = (await customers.GetAllAsync(null, includeInactive: true))[0];
        Assert.Equal(125m, updated.Balance);
    }

    [Fact]
    public async Task ApplyPaymentAsync_DoesNotGoBelowZero()
    {
        var sut = CreateSut(out var customers);
        await sut.CreateAsync(ValidInput());
        var customer = (await customers.GetAllAsync(null, includeInactive: true))[0];
        customer.Balance = 50m;

        await sut.ApplyPaymentAsync(customer.Id, 200m);

        var updated = (await customers.GetAllAsync(null, includeInactive: true))[0];
        Assert.Equal(0m, updated.Balance);
    }

    [Fact]
    public async Task ApplyPaymentAsync_Fails_OnNonPositiveAmount()
    {
        var sut = CreateSut(out _);
        await sut.CreateAsync(ValidInput());

        var result = await sut.ApplyPaymentAsync(Guid.NewGuid(), 0m);

        Assert.False(result.Success);
        Assert.Equal("El abono debe ser mayor a cero.", result.Error);
    }

    [Fact]
    public async Task DeactivateAsync_MarksInactive()
    {
        var sut = CreateSut(out var customers);
        await sut.CreateAsync(ValidInput());
        var customer = (await customers.GetAllAsync(null, includeInactive: true))[0];

        var result = await sut.DeactivateAsync(customer.Id);

        Assert.True(result.Success);
        Assert.False((await customers.GetAllAsync(null, includeInactive: true))[0].IsActive);
        Assert.Empty(await customers.GetAllAsync(null, includeInactive: false));
    }
}